using System.ClientModel;
using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using MassTransit;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Infrastructure.Configuration;
using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Messaging.Consumers;
using ExpenseTracker.Infrastructure.Repositories;
using ExpenseTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Retry;
using Microsoft.Extensions.AI;
using OpenAI;

namespace ExpenseTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment?.EnvironmentName != "Test")
        {
            services.AddDbContext<IAppDbContext, AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        }
        services.AddSignalR();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddTransient<INotificationService, SignalRNotificationService>();
        services.AddAwsS3(configuration);
        services.AddMassTransit(configuration, environment);
        services.AddPollyAndGemini(configuration);
        return services;
    }
    
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"] 
                        ?? throw new InvalidOperationException("JWT Secret is not configured");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
    
    private static IServiceCollection AddAwsS3(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.AddScoped<IFileStorageService, S3FileStorageService>();
        
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = configuration
                .GetSection(AwsS3Options.SectionName)
                .Get<AwsS3Options>() ?? throw new InvalidOperationException("AwsS3Configuration section is missing");

            // Wyciągamy informacje o środowisku bezpośrednio z kontenera DI
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var s3Config = new AmazonS3Config();

            if (env.IsDevelopment())
            {
                s3Config.ServiceURL = options.ServiceUrl;
                s3Config.AuthenticationRegion = options.AuthenticationRegion;
                s3Config.ForcePathStyle = true; // Niezbędne tylko dla MinIO

                var credentials = new BasicAWSCredentials(options.Username, options.Password);
                return new AmazonS3Client(credentials, s3Config);
            }
        
            // --- TRYB PRODUKCYJNY: AWS S3 + Rola IAM z EC2 ---
        
            // Bezpieczny fallback regionu, aby uniknąć błędów inicjalizacji
            var region = !string.IsNullOrWhiteSpace(options.AuthenticationRegion) 
                ? options.AuthenticationRegion 
                : "eu-north-1";

            s3Config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
        
            // Kluczowe: Pusty konstruktor używający wyłącznie s3Config.
            // To zmusza AWS SDK do pominięcia kluczy statycznych i pobrania 
            // automatycznie odnawianych tokenów z profilu maszyny EC2.
            return new AmazonS3Client(s3Config);
        });

        return services;
    }

    private static IServiceCollection AddMassTransit(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var rabbitMqOptions = configuration
            .GetSection(RabbitMqOptions.SectionName)
            .Get<RabbitMqOptions>() ?? new RabbitMqOptions();

        services.AddMassTransit(x =>
        {
            // Konfiguracja Outboxa dla konkretnego DbContextu
            x.AddEntityFrameworkOutbox<AppDbContext>(o =>
            {
                o.UsePostgres();

                // Ten proces działa w tle i przepycha wiadomości z bazy do RabbitMQ
                o.UseBusOutbox();
            });
            
            x.AddConsumer<ReceiptUploadedEventConsumer>();
            if (environment.IsDevelopment())
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqOptions.Host, rabbitMqOptions.VirtualHost, h =>
                    {
                        h.Username(rabbitMqOptions.Username);
                        h.Password(rabbitMqOptions.Password);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                x.UsingAmazonSqs((context, cfg) =>
                {
                    cfg.Host(configuration["AWS:Region"] ?? "eu-central-1", h =>
                    {
                    });

                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }

    private static IServiceCollection AddPollyAndGemini(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddResiliencePipeline("gemini-pipeline", pipelineBuilder =>
        {
            pipelineBuilder.AddRetry(new RetryStrategyOptions
            {
                // W produkcji należy to zawęzić do wyjątków HttpRequestException i statusów 429/50x
                ShouldHandle = new PredicateBuilder().Handle<Exception>(), 
                Delay = TimeSpan.FromSeconds(2),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential
            });
        });

        // 2. Rejestracja i walidacja opcji Gemini
        services.AddOptions<GeminiOptions>()
            .BindConfiguration(GeminiOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey),
                "Brak konfiguracji 'Gemini:ApiKey' w ustawieniach środowiska.")
            .ValidateOnStart();

        var geminiOptions = configuration
            .GetSection(GeminiOptions.SectionName)
            .Get<GeminiOptions>() ?? throw new InvalidOperationException("Brak sekcji 'Gemini' w konfiguracji.");

        if (string.IsNullOrWhiteSpace(geminiOptions.ApiKey))
        {
            throw new InvalidOperationException("Brak konfiguracji 'Gemini:ApiKey' w ustawieniach środowiska.");
        }

        // 3. Utworzenie bazowego klienta wskazującego na endpoint Google
        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(geminiOptions.ApiKey),
            new OpenAIClientOptions 
            { 
                Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai/") 
            });

        // 4. Pobranie instancji dla konkretnego modelu
        var nativeChatClient = openAiClient.GetChatClient("gemini-2.5-flash");

        // 5. Rejestracja standardu Microsoft.Extensions.AI
        services.AddChatClient(nativeChatClient.AsIChatClient());

        return services;
    }
}


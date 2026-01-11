using ExpenseTracker.Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.RegisterServicesFromAssembly(assembly);
        });
        
        services.AddValidatorsFromAssembly(assembly);
        
        return services;
    }
}
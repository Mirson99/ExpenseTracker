using ExpenseTracker.Application.Auth.Responses;
using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Auth.Commands.Login;

public class LoginCommandHandler: IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<LoginCommandHandler> _logger;
    public LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository, ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }
    
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("User with email {Email} does not exist", request.Email);
            throw new NotFoundException("User not found");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("User {UserId} with email {Email} does not match password", user.Id, request.Email);
            throw new UnauthorizedException("Invalid password");
        }
        var token = await _tokenService.CreateToken(user);
        await _refreshTokenRepository.RevokeRefreshTokensForUserAsync(user.Id);
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = _tokenService.GenerateRefreshToken(),
            ExpiresOnUtc = DateTime.UtcNow.AddDays(7),
        };
        _refreshTokenRepository.CreateNewRefreshTokenAsync(refreshToken);
        return new LoginResponse()
        {
            Token = token,
            RefreshToken = refreshToken.Token
        };
    }
}
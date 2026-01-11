using ExpenseTracker.Application.Auth.Responses;
using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Auth.Commands.LoginWithRefreshToken;

public class LoginWithRefreshTokenCommandHandler: IRequestHandler<LoginWithRefreshTokenCommand, LoginResponse>
{
    private IRefreshTokenRepository  _refreshTokenRepository;
    private ITokenService _tokenService;
    
    public LoginWithRefreshTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository, ITokenService tokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
    }
    
    public async Task<LoginResponse> Handle(LoginWithRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        RefreshToken refreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(request.RefreshToken);
        if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            throw new UnauthorizedException("The refresh token has expired.");
        }
        await _refreshTokenRepository.RevokeRefreshTokensForUserAsync(refreshToken.UserId);
        string accessToken = await _tokenService.CreateToken(refreshToken.User);
        refreshToken.Token = _tokenService.GenerateRefreshToken();
        refreshToken.ExpiresOnUtc = DateTime.UtcNow.AddDays(7);
        await _refreshTokenRepository.AddRefreshTokenAsync(refreshToken);
        return new LoginResponse()
        {
            Token = accessToken,
            RefreshToken = refreshToken.Token,
        };
    }
}
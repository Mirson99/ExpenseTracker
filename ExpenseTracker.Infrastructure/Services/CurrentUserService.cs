using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ExpenseTracker.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId => Guid.Parse(
        _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier) 
        ?? throw new UnauthorizedAccessException());

    public string Email => 
        _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.Email) 
        ?? throw new UnauthorizedAccessException();

    public bool IsAuthenticated => 
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
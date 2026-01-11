using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface ITokenService
{
    Task<string> CreateToken(User user);
    string GenerateRefreshToken();
}
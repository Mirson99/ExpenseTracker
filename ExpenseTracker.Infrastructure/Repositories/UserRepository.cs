using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public class UserRepository: IUserRepository
{
    private readonly AppDbContext _authDbContext;
    public UserRepository(AppDbContext authDbContext)
    {
        _authDbContext = authDbContext;
    }
    
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _authDbContext.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _authDbContext.Users.AddAsync(user);
        await _authDbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _authDbContext.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
    }
}
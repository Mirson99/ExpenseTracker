using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Interfaces;

public interface IAppDbContext
{
    public DbSet<User> Users { get; }
    public DbSet<Category> Categories { get; }
    public DbSet<Expense> Expenses { get; }
    public DbSet<RefreshToken> RefreshTokens { get; }
    public DbSet<RecurringExpense> RecurringExpenses { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
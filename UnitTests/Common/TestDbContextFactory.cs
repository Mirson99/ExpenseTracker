using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace UnitTests.Common;
public static class TestDbContextFactory
{
    public static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        
        // Seed categories
        context.Categories.AddRange(
            new Category { Id = 1, Name = "Food", IsSystem = true },
            new Category { Id = 2, Name = "Transport", IsSystem = true }
        );
        context.SaveChanges();
        
        return context;
    }

    public static Mock<ICurrentUserService> CreateMockCurrentUser(Guid userId)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(s => s.UserId).Returns(userId);
        mock.Setup(s => s.IsAuthenticated).Returns(true);
        return mock;
    }
}
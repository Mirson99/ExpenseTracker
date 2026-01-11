using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace UnitTests.Common;

// Tests/Common/MockHelper.cs
public static class MockHelper
{
    public static Mock<IAppDbContext> CreateMockDbContext()
    {
        var mockContext = new Mock<IAppDbContext>();
        var mockExpenses = new Mock<DbSet<Expense>>();
        var mockCategories = new Mock<DbSet<Category>>();
        
        mockContext.Setup(c => c.Expenses).Returns(mockExpenses.Object);
        mockContext.Setup(c => c.Categories).Returns(mockCategories.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        
        return mockContext;
    }

    public static Mock<ICurrentUserService> CreateMockCurrentUser(Guid userId)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(s => s.UserId).Returns(userId);
        return mock;
    }
}
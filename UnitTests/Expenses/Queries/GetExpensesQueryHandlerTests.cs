using ExpenseTracker.Application.Expenses.Queries;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Moq;
using UnitTests.Common;

namespace UnitTests.Expenses.Queries;

public class GetExpensesQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly GetUserExpensesQueryHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public GetExpensesQueryHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockCurrentUser = TestDbContextFactory.CreateMockCurrentUser(_testUserId);
        _handler = new GetUserExpensesQueryHandler(_context, _mockCurrentUser.Object);
        
        // Seed test data
        SeedTestExpenses();
    }

    private void SeedTestExpenses()
    {
        var expenses = new List<Expense>
        {
            new Expense
            {
                Id = Guid.NewGuid(),
                Name = "Expense 1",
                Amount = 100m,
                Date = DateTime.UtcNow.AddDays(-5),
                CategoryId = 1,
                UserId = _testUserId,
                Currency = "PLN",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Expense
            {
                Id = Guid.NewGuid(),
                Name = "Expense 2",
                Amount = 200m,
                Date = DateTime.UtcNow.AddDays(-3),
                CategoryId = 2,
                UserId = _testUserId,
                Currency = "PLN",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Expense
            {
                Id = Guid.NewGuid(),
                Name = "Someone Else's Expense",
                Amount = 500m,
                Date = DateTime.UtcNow,
                CategoryId = 1,
                UserId = Guid.NewGuid(),
                Currency = "PLN",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        _context.Expenses.AddRange(expenses);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_PaginationWorks()
    {
        // Arrange
        var query = new GetUserExpensesQuery(null, null, null, null, Page: 1, PageSize: 1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(2, result.TotalCount);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
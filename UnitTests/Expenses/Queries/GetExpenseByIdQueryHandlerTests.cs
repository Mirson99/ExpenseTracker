using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Expenses.Queries.GetExpenseById;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Moq;
using UnitTests.Common;

namespace UnitTests.Expenses.Queries;

public class GetExpenseByIdQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly GetExpenseByIdQueryHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public GetExpenseByIdQueryHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockCurrentUser = TestDbContextFactory.CreateMockCurrentUser(_testUserId);
        _handler = new GetExpenseByIdQueryHandler(_context, _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Handle_ExpenseExists_ReturnsExpense()
    {
        // Arrange
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Test Expense",
            Description = "Test Description",
            Amount = 150m,
            Date = DateTime.UtcNow,
            CategoryId = 1,
            UserId = _testUserId,
            Currency = "PLN",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var query = new GetExpenseByIdQuery(expense.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expense.Name, result.Name);
        Assert.Equal(expense.Amount, result.Amount);
    }

    [Fact]
    public async Task Handle_ExpenseNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var query = new GetExpenseByIdQuery(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ExpenseBelongsToAnotherUser_ThrowsForbiddenAccessException()
    {
        // Arrange
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Not Mine",
            Amount = 100m,
            Date = DateTime.UtcNow,
            CategoryId = 1,
            UserId = Guid.NewGuid(), // różny user
            Currency = "PLN",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var query = new GetExpenseByIdQuery(expense.Id);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => 
            _handler.Handle(query, CancellationToken.None));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
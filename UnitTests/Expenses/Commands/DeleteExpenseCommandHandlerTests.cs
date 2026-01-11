using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Expenses.Commands.DeleteExpense;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Moq;
using UnitTests.Common;

namespace UnitTests.Expenses;

public class DeleteExpenseCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly DeleteExpenseCommandHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public DeleteExpenseCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockCurrentUser = TestDbContextFactory.CreateMockCurrentUser(_testUserId);
        _handler = new DeleteExpenseCommandHandler(_context, _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_DeletesExpense()
    {
        // Arrange
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            Amount = 50m,
            Date = DateTime.UtcNow,
            CategoryId = 1,
            UserId = _testUserId,
            Currency = "PLN",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var command = new DeleteExpenseCommand(expense.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deleted = await _context.Expenses.FindAsync(expense.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Handle_ExpenseNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new DeleteExpenseCommand(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongUser_ThrowsForbiddenAccessException()
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

        var command = new DeleteExpenseCommand(expense.Id);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
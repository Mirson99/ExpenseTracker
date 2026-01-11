using ExpenseTracker.Application.Expenses.Commands;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using UnitTests.Common;

namespace UnitTests.Expenses;

public class CreateExpenseCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly CreateExpenseCommandHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public CreateExpenseCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockCurrentUser = TestDbContextFactory.CreateMockCurrentUser(_testUserId);
        _handler = new CreateExpenseCommandHandler(_context, _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesExpenseWithCorrectData()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Name: "Grocery Shopping",
            Description: "Weekly groceries",
            Amount: 150.50m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: "PLN"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == result);
        Assert.NotNull(expense);
        Assert.Equal(command.Name, expense.Name);
        Assert.Equal(command.Description, expense.Description);
        Assert.Equal(command.Amount, expense.Amount);
        Assert.Equal(command.CategoryId, expense.CategoryId);
        Assert.Equal(command.Currency, expense.Currency);
        Assert.Equal(_testUserId, expense.UserId);
    }

    [Fact]
    public async Task Handle_NullCurrency_DefaultsToPLN()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Name: "Test Expense",
            Description: "Test",
            Amount: 100m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var expense = await _context.Expenses.FindAsync(result);
        Assert.Equal("PLN", expense.Currency);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsCreatedAndUpdatedAt()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Name: "Test",
            Description: "Test",
            Amount: 50m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: "USD"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var expense = await _context.Expenses.FindAsync(result);
        Assert.NotEqual(default(DateTime), expense.CreatedAt);
        Assert.NotEqual(default(DateTime), expense.UpdatedAt);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
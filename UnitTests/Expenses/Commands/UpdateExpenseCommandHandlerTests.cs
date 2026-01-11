using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Expenses.Commands.UpdateExpense;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using UnitTests.Common;

namespace UnitTests.Expenses;

public class UpdateExpenseCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly UpdateExpenseCommandHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public UpdateExpenseCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockCurrentUser = TestDbContextFactory.CreateMockCurrentUser(_testUserId);
        _handler = new UpdateExpenseCommandHandler(_context, _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesExpense()
    {
        // Arrange
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Description = "Original Desc",
            Amount = 100m,
            Date = DateTime.UtcNow.AddDays(-1),
            CategoryId = 1,
            UserId = _testUserId,
            Currency = "PLN",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var command = new UpdateExpenseCommand(
            Id: expense.Id,
            Name: "Updated Name",
            Description: "Updated Desc",
            Amount: 200m,
            Date: DateTime.UtcNow,
            CategoryId: 2,
            Currency: "USD"
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _context.Expenses.FindAsync(expense.Id);
        Assert.Equal(command.Name, updated.Name);
        Assert.Equal(command.Description, updated.Description);
        Assert.Equal(command.Amount, updated.Amount);
        Assert.Equal(command.CategoryId, updated.CategoryId);
        Assert.Equal(command.Currency, updated.Currency);
    }

    [Fact]
    public async Task Handle_ExpenseNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new UpdateExpenseCommand(
            Id: Guid.NewGuid(),
            Name: "Test",
            Description: "Test",
            Amount: 100m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: "PLN"
        );

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongUser_ThrowsForbiddenAccessException()
    {
        // Arrange
        var differentUserId = Guid.NewGuid();
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Someone Else's Expense",
            Amount = 100m,
            Date = DateTime.UtcNow,
            CategoryId = 1,
            UserId = differentUserId, // różny user
            Currency = "PLN",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var command = new UpdateExpenseCommand(
            Id: expense.Id,
            Name: "Hacked",
            Description: "Hacked",
            Amount: 999m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: "PLN"
        );

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
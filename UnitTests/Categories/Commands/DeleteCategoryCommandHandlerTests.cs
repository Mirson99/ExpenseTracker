using ExpenseTracker.Application.Categories.Commands.DeleteCategory;
using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Moq;
using UnitTests.Common;

namespace UnitTests.Categories.Commands;

// DeleteCategoryCommandHandler Tests
public class DeleteCategoryCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly DeleteCategoryCommandHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public DeleteCategoryCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockCurrentUser = TestDbContextFactory.CreateMockCurrentUser(_testUserId);
        _handler = new DeleteCategoryCommandHandler(_context, _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_DeletesCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = 10,
            Name = "My Custom Category",
            UserId = _testUserId,
            IsSystem = false
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var command = new DeleteCategoryCommand(category.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deleted = await _context.Categories.FindAsync(category.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new DeleteCategoryCommand(999);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongUser_ThrowsForbiddenAccessException()
    {
        // Arrange
        var category = new Category
        {
            Id = 10,
            Name = "Someone Else's Category",
            UserId = Guid.NewGuid(), // różny user
            IsSystem = false
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var command = new DeleteCategoryCommand(category.Id);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SystemCategory_ThrowsForbiddenAccessException()
    {
        // Arrange
        var command = new DeleteCategoryCommand(1); // system category

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CategoryHasExpenses_ThrowsValidationException()
    {
        // Arrange
        var category = new Category
        {
            Id = 10,
            Name = "Category With Expenses",
            UserId = _testUserId,
            IsSystem = false
        };
        _context.Categories.Add(category);
        
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = "Test Expense",
            Amount = 100m,
            Date = DateTime.UtcNow,
            CategoryId = category.Id,
            UserId = _testUserId,
            Currency = "PLN",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var command = new DeleteCategoryCommand(category.Id);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
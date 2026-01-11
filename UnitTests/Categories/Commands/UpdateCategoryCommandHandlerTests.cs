using ExpenseTracker.Application.Categories.Commands.UpdateCategory;
using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Moq;
using UnitTests.Common;

namespace UnitTests.Categories.Commands;

public class UpdateCategoryCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly UpdateCategoryCommandHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public UpdateCategoryCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockCurrentUser = TestDbContextFactory.CreateMockCurrentUser(_testUserId);
        _handler = new UpdateCategoryCommandHandler(_context, _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = 10,
            Name = "Original Name",
            UserId = _testUserId,
            IsSystem = false
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var command = new UpdateCategoryCommand(
            Id: category.Id,
            Name: "Updated Name"
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _context.Categories.FindAsync(category.Id);
        Assert.Equal(command.Name, updated.Name);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new UpdateCategoryCommand(Id: 999, Name: "Test");

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
            Name = "Not Mine",
            UserId = Guid.NewGuid(), // różny user
            IsSystem = false
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var command = new UpdateCategoryCommand(
            Id: category.Id,
            Name: "Hacked Name"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SystemCategory_ThrowsForbiddenAccessException()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Id: 1, // system category
            Name: "Hacked System Category"
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
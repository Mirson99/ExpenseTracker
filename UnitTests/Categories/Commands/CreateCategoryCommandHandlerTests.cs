using ExpenseTracker.Application.Categories.Commands;
using ExpenseTracker.Application.Categories.Commands.CreateCategory;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using UnitTests.Common;

namespace UnitTests.Categories.Commands;

public class CreateCategoryCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly CreateCategoryCommandHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public CreateCategoryCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockCurrentUser = TestDbContextFactory.CreateMockCurrentUser(_testUserId);
        _handler = new CreateCategoryCommandHandler(_context, _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesCustomCategory()
    {
        // Arrange
        var command = new CreateCategoryCommand(Name: "My Custom Category");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == command.Name);
        Assert.NotNull(category);
        Assert.Equal(_testUserId, category.UserId);
        Assert.False(category.IsSystem);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
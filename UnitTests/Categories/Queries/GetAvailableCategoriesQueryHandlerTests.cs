using ExpenseTracker.Application.Categories.Queries.GetAvailableCategories;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using Moq;
using UnitTests.Common;

namespace UnitTests.Categories.Queries;

// GetAvailableCategoriesQueryHandler Tests
public class GetAvailableCategoriesQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly GetAvailableCategoriesQueryHandler _handler;
    private readonly Guid _testUserId = Guid.NewGuid();

    public GetAvailableCategoriesQueryHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockCurrentUser = TestDbContextFactory.CreateMockCurrentUser(_testUserId);
        _handler = new GetAvailableCategoriesQueryHandler(_context, _mockCurrentUser.Object);
        
        _context.Categories.Add(new Category
        {
            Id = 10,
            Name = "My Custom Category",
            UserId = _testUserId,
            IsSystem = false
        });
        
        _context.Categories.Add(new Category
        {
            Id = 11,
            Name = "Someone Else's Category",
            UserId = Guid.NewGuid(),
            IsSystem = false
        });
        
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ReturnsSystemAndUserCategories()
    {
        // Arrange
        var query = new GetAvailableCategoriesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Contains(result, c => c.Name == "Food");
        Assert.Contains(result, c => c.Name == "Transport");
        Assert.Contains(result, c => c.Name == "My Custom Category");
        Assert.DoesNotContain(result, c => c.Name == "Someone Else's Category");
    }

    [Fact]
    public async Task Handle_ReturnsOnlySystemCategoriesWhenUserHasNoCustom()
    {
        // Arrange
        var newUserId = Guid.NewGuid();
        var newMockUser = TestDbContextFactory.CreateMockCurrentUser(newUserId);
        var newHandler = new GetAvailableCategoriesQueryHandler(_context, newMockUser.Object);
        var query = new GetAvailableCategoriesQuery();

        // Act
        var result = await newHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(result, c => c.Name == "My Custom Category");
    }

    [Fact]
    public async Task Handle_ReturnsIdsAndNames()
    {
        // Arrange
        var query = new GetAvailableCategoriesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.All(result, c => 
        {
            Assert.NotEqual(0, c.Id);
            Assert.False(string.IsNullOrEmpty(c.Name));
        });
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
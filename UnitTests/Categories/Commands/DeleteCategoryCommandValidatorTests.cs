using ExpenseTracker.Application.Categories.Commands.DeleteCategory;

namespace UnitTests.Categories.Commands;

// DeleteCategoryCommandValidator Tests
public class DeleteCategoryCommandValidatorTests
{
    private readonly DeleteCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Id: 10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidId_FailsValidation(int id)
    {
        // Arrange
        var command = new DeleteCategoryCommand(Id: id);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Id));
    }
}
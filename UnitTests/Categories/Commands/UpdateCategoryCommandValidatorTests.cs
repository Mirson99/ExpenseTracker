using ExpenseTracker.Application.Categories.Commands.UpdateCategory;

namespace UnitTests.Categories.Commands;

// UpdateCategoryCommandValidator Tests
public class UpdateCategoryCommandValidatorTests
{
    private readonly UpdateCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new UpdateCategoryCommand(Id: 10, Name: "Updated Category");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidId_FailsValidation()
    {
        // Arrange
        var command = new UpdateCategoryCommand(Id: 0, Name: "Test");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Id));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_FailsValidation(string name)
    {
        // Arrange
        var command = new UpdateCategoryCommand(Id: 10, Name: name);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void Validate_NameTooLong_FailsValidation()
    {
        // Arrange
        var command = new UpdateCategoryCommand(Id: 10, Name: new string('a', 51));

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Name));
    }
}
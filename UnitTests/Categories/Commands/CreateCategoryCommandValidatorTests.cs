using ExpenseTracker.Application.Categories.Commands;
using ExpenseTracker.Application.Categories.Commands.CreateCategory;

namespace UnitTests.Categories.Commands;

public class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new CreateCategoryCommand(Name: "My Custom Category");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_FailsValidation(string name)
    {
        // Arrange
        var command = new CreateCategoryCommand(Name: name);

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
        var command = new CreateCategoryCommand(Name: new string('a', 51)); // 51 characters

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Name));
    }
}
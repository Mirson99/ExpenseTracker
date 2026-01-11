using ExpenseTracker.Application.Expenses.Commands;

namespace UnitTests.Expenses;

// CreateExpenseCommandValidator Tests
public class CreateExpenseCommandValidatorTests
{
    private readonly CreateExpenseCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Name: "Valid Expense",
            Description: "Test description",
            Amount: 50m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: "PLN"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_FailsValidation(string name)
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Name: name,
            Description: "Test",
            Amount: 50m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: "PLN"
        );

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
        var command = new CreateExpenseCommand(
            Name: new string('a', 101), // 101 characters
            Description: "Test",
            Amount: 50m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: "PLN"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Validate_InvalidAmount_FailsValidation(decimal amount)
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Name: "Test",
            Description: "Test",
            Amount: amount,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: "PLN"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Amount));
    }

    [Fact]
    public void Validate_DescriptionTooLong_FailsValidation()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Name: "Test",
            Description: new string('a', 1001), // 1001 characters
            Amount: 100m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: "PLN"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Description));
    }

    [Theory]
    [InlineData("PLN")]
    [InlineData("USD")]
    [InlineData("EUR")]
    public void Validate_ValidCurrencyCode_PassesValidation(string currency)
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Name: "Test",
            Description: "Test",
            Amount: 100m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: currency
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("usd")]
    [InlineData("123")]
    public void Validate_InvalidCurrencyCode_FailsValidation(string currency)
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Name: "Test",
            Description: "Test",
            Amount: 100m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: currency
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Currency));
    }

    [Fact]
    public void Validate_NullCurrency_PassesValidation()
    {
        // Arrange
        var command = new CreateExpenseCommand(
            Name: "Test",
            Description: "Test",
            Amount: 100m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}
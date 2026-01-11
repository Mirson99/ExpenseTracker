using ExpenseTracker.Application.Expenses.Commands.UpdateExpense;

namespace UnitTests.Expenses;

public class UpdateExpenseCommandValidatorTests
{
    private readonly UpdateExpenseCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new UpdateExpenseCommand(
            Id: Guid.NewGuid(),
            Name: "Updated Expense",
            Description: "Updated description",
            Amount: 150m,
            Date: DateTime.UtcNow,
            CategoryId: 2,
            Currency: "USD"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_FailsValidation()
    {
        // Arrange
        var command = new UpdateExpenseCommand(
            Id: Guid.Empty,
            Name: "Test",
            Description: "Test",
            Amount: 100m,
            Date: DateTime.UtcNow,
            CategoryId: 1,
            Currency: "PLN"
        );

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
        var command = new UpdateExpenseCommand(
            Id: Guid.NewGuid(),
            Name: name,
            Description: "Test",
            Amount: 100m,
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
    [InlineData(-50)]
    public void Validate_InvalidAmount_FailsValidation(decimal amount)
    {
        // Arrange
        var command = new UpdateExpenseCommand(
            Id: Guid.NewGuid(),
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
    public void Validate_FutureDate_FailsValidation()
    {
        // Arrange
        var command = new UpdateExpenseCommand(
            Id: Guid.NewGuid(),
            Name: "Test",
            Description: "Test",
            Amount: 100m,
            Date: DateTime.UtcNow.AddDays(5),
            CategoryId: 1,
            Currency: "PLN"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Date));
    }
}
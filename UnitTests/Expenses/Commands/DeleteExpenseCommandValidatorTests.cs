using ExpenseTracker.Application.Expenses.Commands.DeleteExpense;

namespace UnitTests.Expenses;

public class DeleteExpenseCommandValidatorTests
{
    private readonly DeleteExpenseCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new DeleteExpenseCommand(Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_FailsValidation()
    {
        // Arrange
        var command = new DeleteExpenseCommand(Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.Id));
    }
}
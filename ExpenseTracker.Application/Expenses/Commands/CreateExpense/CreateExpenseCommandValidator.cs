using FluentValidation;
namespace ExpenseTracker.Application.Expenses.Commands;

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name is too long");

        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount is required")
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description is too long");
        
        When(x => x.IsRecurring, () =>
        {
            RuleFor(x => x.Frequency)
                .NotNull().WithMessage("Frequency is required for recurring expenses")
                .IsInEnum().WithMessage("Invalid frequency value");
        });
    }
}
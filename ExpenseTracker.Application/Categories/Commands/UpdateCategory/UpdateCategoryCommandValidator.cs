using FluentValidation;

namespace ExpenseTracker.Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name is too long");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
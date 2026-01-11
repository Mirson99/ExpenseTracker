using FluentValidation;

namespace ExpenseTracker.Application.Auth.Commands.LoginWithRefreshToken;

public class LoginWithRefreshTokenCommandValidator : AbstractValidator<LoginWithRefreshTokenCommand>
{
    public LoginWithRefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required")
            .MinimumLength(32).WithMessage("Invalid refresh token format");
    }
}
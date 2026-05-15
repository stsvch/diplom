// ResetPasswordCommandValidator.cs
using FluentValidation;

namespace Auth.Application.Commands.ResetPassword;

// Валидатор требует email, token восстановления и новый пароль с минимальной сложностью.
public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.");
    }
}

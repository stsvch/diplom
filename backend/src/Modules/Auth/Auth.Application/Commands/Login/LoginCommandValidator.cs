// LoginCommandValidator.cs
using FluentValidation;

namespace Auth.Application.Commands.Login;

// Валидатор отсекает пустой/некорректный email и пустой пароль до обращения к Identity.
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

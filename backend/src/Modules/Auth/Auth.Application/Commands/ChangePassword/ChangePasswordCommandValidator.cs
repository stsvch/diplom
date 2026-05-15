// ChangePasswordCommandValidator.cs
using FluentValidation;

namespace Auth.Application.Commands.ChangePassword;

// Валидатор требует старый и новый пароль перед сменой пароля текущего пользователя.
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}

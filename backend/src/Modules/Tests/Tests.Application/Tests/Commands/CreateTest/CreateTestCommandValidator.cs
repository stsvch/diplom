using FluentValidation;

namespace Tests.Application.Tests.Commands.CreateTest;

public class CreateTestCommandValidator : AbstractValidator<CreateTestCommand>
{
    public CreateTestCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название теста обязательно.")
            .MaximumLength(200).WithMessage("Название теста не должно превышать 200 символов.");

        RuleFor(x => x.CreatedById)
            .NotEmpty().WithMessage("Идентификатор автора обязателен.");

        RuleFor(x => x.TimeLimitMinutes)
            .GreaterThan(0).When(x => x.TimeLimitMinutes.HasValue)
            .WithMessage("Лимит времени должен быть больше 0.");

        RuleFor(x => x.MaxAttempts)
            .GreaterThan(0).When(x => x.MaxAttempts.HasValue)
            .WithMessage("Количество попыток должно быть больше 0.");
    }
}

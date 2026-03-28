using FluentValidation;

namespace Assignments.Application.Assignments.Commands.CreateAssignment;

public class CreateAssignmentCommandValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название задания обязательно.")
            .MaximumLength(200).WithMessage("Название задания не должно превышать 200 символов.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание задания обязательно.");

        RuleFor(x => x.MaxScore)
            .GreaterThan(0).WithMessage("Максимальный балл должен быть больше 0.");

        RuleFor(x => x.MaxAttempts)
            .GreaterThan(0).When(x => x.MaxAttempts.HasValue)
            .WithMessage("Количество попыток должно быть больше 0.");

        RuleFor(x => x.CreatedById)
            .NotEmpty().WithMessage("Идентификатор автора обязателен.");
    }
}

// CreateAssignmentCommandValidator.cs

using FluentValidation;

namespace Assignments.Application.Assignments.Commands.CreateAssignment;

/// <summary>
/// Валидатор проверяет обязательную связь с курсом, название, автора, баллы и лимит попыток.
/// </summary>
public class CreateAssignmentCommandValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Задание должно быть привязано к курсу.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название задания обязательно.")
            .MaximumLength(200).WithMessage("Название задания не должно превышать 200 символов.");

        // Description допустим пустым на этапе создания — задание создаётся как Draft.
        // Строгая проверка переносится на момент перехода в Ready / при публикации курса.

        RuleFor(x => x.MaxScore)
            .GreaterThanOrEqualTo(0).WithMessage("Максимальный балл не может быть отрицательным.");

        RuleFor(x => x.MaxAttempts)
            .GreaterThan(0).When(x => x.MaxAttempts.HasValue)
            .WithMessage("Количество попыток должно быть больше 0.");

        RuleFor(x => x.CreatedById)
            .NotEmpty().WithMessage("Идентификатор автора обязателен.");
    }
}

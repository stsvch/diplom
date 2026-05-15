// CreateDisciplineCommandValidator.cs

using FluentValidation;

namespace Courses.Application.Disciplines.Commands.CreateDiscipline;

// Тип class: ключевой элемент файла CreateDisciplineCommandValidator.cs.
public class CreateDisciplineCommandValidator : AbstractValidator<CreateDisciplineCommand>
{
    // Правила ниже защищают handler от некорректной входной модели.
    public CreateDisciplineCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название дисциплины обязательно.")
            .MaximumLength(100).WithMessage("Название дисциплины не должно превышать 100 символов.");
    }
}

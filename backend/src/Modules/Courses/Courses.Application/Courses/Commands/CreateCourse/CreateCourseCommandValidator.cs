// CreateCourseCommandValidator.cs

using FluentValidation;

namespace Courses.Application.Courses.Commands.CreateCourse;

// Тип class: ключевой элемент файла CreateCourseCommandValidator.cs.
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    // Правила ниже защищают handler от некорректной входной модели.
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название курса обязательно.")
            .MaximumLength(200).WithMessage("Название курса не должно превышать 200 символов.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание курса обязательно.");

        RuleFor(x => x.DisciplineId)
            .NotEmpty().WithMessage("Дисциплина обязательна.");
    }
}

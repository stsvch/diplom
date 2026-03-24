using FluentValidation;

namespace Courses.Application.Lessons.Commands.CreateLesson;

public class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название урока обязательно.")
            .MaximumLength(200).WithMessage("Название урока не должно превышать 200 символов.");

        RuleFor(x => x.ModuleId)
            .NotEmpty().WithMessage("Модуль обязателен.");
    }
}

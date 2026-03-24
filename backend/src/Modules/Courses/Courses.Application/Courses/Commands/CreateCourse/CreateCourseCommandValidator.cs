using FluentValidation;

namespace Courses.Application.Courses.Commands.CreateCourse;

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
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

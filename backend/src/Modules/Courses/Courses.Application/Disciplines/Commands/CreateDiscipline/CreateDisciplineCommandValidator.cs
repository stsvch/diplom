using FluentValidation;

namespace Courses.Application.Disciplines.Commands.CreateDiscipline;

public class CreateDisciplineCommandValidator : AbstractValidator<CreateDisciplineCommand>
{
    public CreateDisciplineCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название дисциплины обязательно.")
            .MaximumLength(100).WithMessage("Название дисциплины не должно превышать 100 символов.");
    }
}

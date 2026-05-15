// CreateModuleCommandValidator.cs

using FluentValidation;

namespace Courses.Application.Modules.Commands.CreateModule;

// Тип class: ключевой элемент файла CreateModuleCommandValidator.cs.
public class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    // Правила ниже защищают handler от некорректной входной модели.
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название модуля обязательно.")
            .MaximumLength(200).WithMessage("Название модуля не должно превышать 200 символов.");

        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("Курс обязателен.");
    }
}

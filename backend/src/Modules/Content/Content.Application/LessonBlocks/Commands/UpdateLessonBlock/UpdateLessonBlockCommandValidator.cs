// UpdateLessonBlockCommandValidator.cs

using FluentValidation;

namespace Content.Application.LessonBlocks.Commands.UpdateLessonBlock;

// Тип class: ключевой элемент файла UpdateLessonBlockCommandValidator.cs.
public class UpdateLessonBlockCommandValidator : AbstractValidator<UpdateLessonBlockCommand>
{
    // Правила ниже защищают handler от некорректной входной модели.
    public UpdateLessonBlockCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Data).NotNull();
    }
}

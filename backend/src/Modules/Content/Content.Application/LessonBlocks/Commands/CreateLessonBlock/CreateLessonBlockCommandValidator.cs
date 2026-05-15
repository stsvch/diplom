// CreateLessonBlockCommandValidator.cs

using FluentValidation;

namespace Content.Application.LessonBlocks.Commands.CreateLessonBlock;

// Тип class: ключевой элемент файла CreateLessonBlockCommandValidator.cs.
public class CreateLessonBlockCommandValidator : AbstractValidator<CreateLessonBlockCommand>
{
    // Правила ниже защищают handler от некорректной входной модели.
    public CreateLessonBlockCommandValidator()
    {
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.Data).NotNull();
    }
}

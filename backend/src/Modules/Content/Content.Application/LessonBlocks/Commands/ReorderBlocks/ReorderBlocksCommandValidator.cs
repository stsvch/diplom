// ReorderBlocksCommandValidator.cs

using FluentValidation;

namespace Content.Application.LessonBlocks.Commands.ReorderBlocks;

// Тип class: ключевой элемент файла ReorderBlocksCommandValidator.cs.
public class ReorderBlocksCommandValidator : AbstractValidator<ReorderBlocksCommand>
{
    // Правила ниже защищают handler от некорректной входной модели.
    public ReorderBlocksCommandValidator()
    {
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.OrderedIds).NotEmpty();
    }
}

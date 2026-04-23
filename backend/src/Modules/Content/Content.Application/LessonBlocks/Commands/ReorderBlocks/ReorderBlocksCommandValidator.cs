using FluentValidation;

namespace Content.Application.LessonBlocks.Commands.ReorderBlocks;

public class ReorderBlocksCommandValidator : AbstractValidator<ReorderBlocksCommand>
{
    public ReorderBlocksCommandValidator()
    {
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.OrderedIds).NotEmpty();
    }
}

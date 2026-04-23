using FluentValidation;

namespace Content.Application.LessonBlocks.Commands.UpdateLessonBlock;

public class UpdateLessonBlockCommandValidator : AbstractValidator<UpdateLessonBlockCommand>
{
    public UpdateLessonBlockCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Data).NotNull();
    }
}

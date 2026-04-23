using FluentValidation;

namespace Content.Application.LessonBlocks.Commands.CreateLessonBlock;

public class CreateLessonBlockCommandValidator : AbstractValidator<CreateLessonBlockCommand>
{
    public CreateLessonBlockCommandValidator()
    {
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.Data).NotNull();
    }
}

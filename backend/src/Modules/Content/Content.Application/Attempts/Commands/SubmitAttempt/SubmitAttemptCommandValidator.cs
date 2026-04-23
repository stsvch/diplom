using FluentValidation;

namespace Content.Application.Attempts.Commands.SubmitAttempt;

public class SubmitAttemptCommandValidator : AbstractValidator<SubmitAttemptCommand>
{
    public SubmitAttemptCommandValidator()
    {
        RuleFor(x => x.BlockId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Answers).NotNull();
    }
}

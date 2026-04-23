using FluentValidation;

namespace Content.Application.Attempts.Commands.ReviewAttempt;

public class ReviewAttemptCommandValidator : AbstractValidator<ReviewAttemptCommand>
{
    public ReviewAttemptCommandValidator()
    {
        RuleFor(x => x.AttemptId).NotEmpty();
        RuleFor(x => x.ReviewerId).NotEmpty();
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
    }
}

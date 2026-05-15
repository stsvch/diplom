// ReviewAttemptCommandValidator.cs

using FluentValidation;

namespace Content.Application.Attempts.Commands.ReviewAttempt;

// Тип class: ключевой элемент файла ReviewAttemptCommandValidator.cs.
public class ReviewAttemptCommandValidator : AbstractValidator<ReviewAttemptCommand>
{
    // Правила ниже защищают handler от некорректной входной модели.
    public ReviewAttemptCommandValidator()
    {
        RuleFor(x => x.AttemptId).NotEmpty();
        RuleFor(x => x.ReviewerId).NotEmpty();
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
    }
}

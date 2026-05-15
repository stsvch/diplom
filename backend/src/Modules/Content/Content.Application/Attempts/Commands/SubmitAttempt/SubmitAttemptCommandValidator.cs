// SubmitAttemptCommandValidator.cs

using FluentValidation;

namespace Content.Application.Attempts.Commands.SubmitAttempt;

// Тип class: ключевой элемент файла SubmitAttemptCommandValidator.cs.
public class SubmitAttemptCommandValidator : AbstractValidator<SubmitAttemptCommand>
{
    // Правила ниже защищают handler от некорректной входной модели.
    public SubmitAttemptCommandValidator()
    {
        RuleFor(x => x.BlockId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Answers).NotNull();
    }
}

using FluentValidation;
using Tests.Domain.Enums;

namespace Tests.Application.Tests.Commands.AddQuestion;

public class AddQuestionCommandValidator : AbstractValidator<AddQuestionCommand>
{
    public AddQuestionCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст вопроса обязателен.");

        RuleFor(x => x.Points)
            .GreaterThan(0).WithMessage("Баллы за вопрос должны быть больше 0.");

        RuleFor(x => x.AnswerOptions)
            .NotEmpty().WithMessage("Необходимо указать хотя бы один вариант ответа.")
            .When(x => x.Type != QuestionType.OpenAnswer);

        RuleFor(x => x.AnswerOptions)
            .Must(options => options.Any(o => o.IsCorrect))
            .WithMessage("Необходимо указать хотя бы один правильный ответ.")
            .When(x => x.Type == QuestionType.SingleChoice || x.Type == QuestionType.MultipleChoice || x.Type == QuestionType.TextInput);

        RuleFor(x => x.CreatedById)
            .NotEmpty().WithMessage("Идентификатор автора обязателен.");

        RuleFor(x => x.TestId)
            .NotEmpty().WithMessage("Идентификатор теста обязателен.");
    }
}

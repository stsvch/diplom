using FluentValidation;

namespace Tests.Application.Tests.Commands.AddQuestion;

public class AddQuestionCommandValidator : AbstractValidator<AddQuestionCommand>
{
    public AddQuestionCommandValidator()
    {
        // Вопрос создаётся «черновиком» из UI — текст и варианты заполняются потом
        // через UpdateQuestion. Поэтому здесь валидируем только обязательные базовые поля,
        // а полноту контента проверяем при готовности теста (CourseBuilder readiness).

        RuleFor(x => x.Points)
            .GreaterThan(0).WithMessage("Баллы за вопрос должны быть больше 0.");

        RuleFor(x => x.CreatedById)
            .NotEmpty().WithMessage("Идентификатор автора обязателен.");

        RuleFor(x => x.TestId)
            .NotEmpty().WithMessage("Идентификатор теста обязателен.");
    }
}

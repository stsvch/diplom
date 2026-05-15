// DeleteQuestionCommandHandler.cs

using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.Interfaces;

namespace Tests.Application.Tests.Commands.DeleteQuestion;

/// <summary>
/// Обработчик CQRS-команды DeleteQuestionCommand: выполняет сценарий изменения состояния и сохраняет результат.
/// </summary>
public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand, Result<string>>
{
    private readonly ITestsDbContext _context;

    public DeleteQuestionCommandHandler(ITestsDbContext context)
    {
        _context = context;
    }

    // Основной сценарий handler-а: проверки, чтение/изменение данных и возврат результата.
    public async Task<Result<string>> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .Include(q => q.Test)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (question is null)
            return Result.Failure<string>("Вопрос не найден.");

        if (question.Test.CreatedById != request.CreatedById)
            return Result.Failure<string>("Вы не являетесь автором этого теста.");

        var test = question.Test;

        _context.Questions.Remove(question);

        // Пересчитываем максимальный балл теста после изменения вопросов.
        var remainingPoints = await _context.Questions
            .Where(q => q.TestId == test.Id && q.Id != question.Id)
            .SumAsync(q => q.Points, cancellationToken);
        test.MaxScore = remainingPoints;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Вопрос успешно удалён.");
    }
}

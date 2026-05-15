// SaveAnswerCommandHandler.cs

using System.Text.Json;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.Interfaces;
using Tests.Domain.Entities;
using Tests.Domain.Enums;

namespace Tests.Application.Attempts.Commands.SaveAnswer;

/// <summary>
/// Обработчик CQRS-команды SaveAnswerCommand: выполняет сценарий изменения состояния и сохраняет результат.
/// </summary>
public class SaveAnswerCommandHandler : IRequestHandler<SaveAnswerCommand, Result<string>>
{
    private readonly ITestsDbContext _context;

    public SaveAnswerCommandHandler(ITestsDbContext context)
    {
        _context = context;
    }

    // Основной сценарий handler-а: проверки, чтение/изменение данных и возврат результата.
    public async Task<Result<string>> Handle(SaveAnswerCommand request, CancellationToken cancellationToken)
    {
        var attempt = await _context.TestAttempts
            .Include(a => a.Test)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken);

        if (attempt is null)
            return Result.Failure<string>("Попытка не найдена.");

        if (attempt.StudentId != request.StudentId)
            return Result.Failure<string>("Эта попытка не принадлежит вам.");

        if (attempt.Status != AttemptStatus.InProgress)
            return Result.Failure<string>("Попытка уже завершена.");

        // Проверяем лимит времени на сервере, чтобы клиент не мог обойти таймер.
        if (attempt.Test.TimeLimitMinutes.HasValue)
        {
            var elapsed = DateTime.UtcNow - attempt.StartedAt;
            if (elapsed > TimeSpan.FromMinutes(attempt.Test.TimeLimitMinutes.Value))
                return Result.Failure<string>("Время на тест вышло — ответ не сохранён.");
        }

        // Проверяем, что вопрос действительно принадлежит этому тесту.
        var questionExists = await _context.Questions
            .AnyAsync(q => q.Id == request.QuestionId && q.TestId == attempt.TestId, cancellationToken);

        if (!questionExists)
            return Result.Failure<string>("Вопрос не найден в данном тесте.");

        // Обновляем существующий ответ или создаём новый.
        var existingResponse = await _context.TestResponses
            .FirstOrDefaultAsync(r => r.AttemptId == request.AttemptId
                                   && r.QuestionId == request.QuestionId, cancellationToken);

        var selectedIdsJson = request.SelectedOptionIds != null
            ? JsonSerializer.Serialize(request.SelectedOptionIds)
            : null;

        if (existingResponse != null)
        {
            existingResponse.SelectedOptionIds = selectedIdsJson;
            existingResponse.TextAnswer = request.TextAnswer;
        }
        else
        {
            var response = new TestResponse
            {
                AttemptId = request.AttemptId,
                QuestionId = request.QuestionId,
                SelectedOptionIds = selectedIdsJson,
                TextAnswer = request.TextAnswer
            };
            _context.TestResponses.Add(response);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Ответ сохранён.");
    }
}

using System.Text.Json;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.Interfaces;
using Tests.Domain.Entities;
using Tests.Domain.Enums;

namespace Tests.Application.Attempts.Commands.SaveAnswer;

public class SaveAnswerCommandHandler : IRequestHandler<SaveAnswerCommand, Result<string>>
{
    private readonly ITestsDbContext _context;

    public SaveAnswerCommandHandler(ITestsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(SaveAnswerCommand request, CancellationToken cancellationToken)
    {
        var attempt = await _context.TestAttempts
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken);

        if (attempt is null)
            return Result.Failure<string>("Попытка не найдена.");

        if (attempt.StudentId != request.StudentId)
            return Result.Failure<string>("Эта попытка не принадлежит вам.");

        if (attempt.Status != AttemptStatus.InProgress)
            return Result.Failure<string>("Попытка уже завершена.");

        // Check question exists in the test
        var questionExists = await _context.Questions
            .AnyAsync(q => q.Id == request.QuestionId && q.TestId == attempt.TestId, cancellationToken);

        if (!questionExists)
            return Result.Failure<string>("Вопрос не найден в данном тесте.");

        // Find existing response or create new
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

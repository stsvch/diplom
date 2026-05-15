// GetPendingTestAttemptsQueryHandler.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;
using Tests.Domain.Enums;

namespace Tests.Application.Attempts.Queries.GetPendingAttempts;

/// <summary>
/// Обработчик CQRS-запроса GetPendingTestAttemptsQuery: читает данные, применяет фильтры и возвращает DTO/Result.
/// </summary>
public class GetPendingTestAttemptsQueryHandler : IRequestHandler<GetPendingTestAttemptsQuery, List<PendingTestAttemptDto>>
{
    private readonly ITestsDbContext _context;

    public GetPendingTestAttemptsQueryHandler(ITestsDbContext context)
    {
        _context = context;
    }

    // Основной сценарий handler-а: проверки, чтение/изменение данных и возврат результата.
    public async Task<List<PendingTestAttemptDto>> Handle(GetPendingTestAttemptsQuery request, CancellationToken cancellationToken)
    {
        var attempts = await _context.TestAttempts
            .AsNoTracking()
            .Where(a => a.Status == AttemptStatus.NeedsReview && a.Test.CreatedById == request.TeacherId)
            .Select(a => new
            {
                a.Id,
                a.TestId,
                TestTitle = a.Test.Title,
                a.StudentId,
                a.AttemptNumber,
                a.StartedAt,
                a.CompletedAt,
                a.Score,
                MaxScore = a.Test.MaxScore,
                Responses = a.Responses
                    .Where(r => r.Question.GradeType == QuestionGradeType.Manual)
                    .Select(r => new { r.IsCorrect })
                    .ToList(),
            })
            .OrderBy(a => a.CompletedAt ?? a.StartedAt)
            .ToListAsync(cancellationToken);

        return attempts
            .Select(a => new PendingTestAttemptDto
            {
                Id = a.Id,
                TestId = a.TestId,
                TestTitle = a.TestTitle,
                StudentId = a.StudentId,
                AttemptNumber = a.AttemptNumber,
                StartedAt = a.StartedAt,
                CompletedAt = a.CompletedAt,
                Score = a.Score,
                MaxScore = a.MaxScore,
                OpenQuestionsTotal = a.Responses.Count,
                OpenQuestionsUngraded = a.Responses.Count(r => r.IsCorrect == null),
            })
            .ToList();
    }
}

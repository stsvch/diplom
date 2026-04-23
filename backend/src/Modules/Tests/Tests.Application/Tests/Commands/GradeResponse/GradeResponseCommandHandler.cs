using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.Interfaces;
using Tests.Domain.Enums;

namespace Tests.Application.Tests.Commands.GradeResponse;

public class GradeResponseCommandHandler : IRequestHandler<GradeResponseCommand, Result<string>>
{
    private readonly ITestsDbContext _context;
    private readonly IGradeRecordWriter _grades;
    private readonly INotificationDispatcher _notifications;

    public GradeResponseCommandHandler(
        ITestsDbContext context,
        IGradeRecordWriter grades,
        INotificationDispatcher notifications)
    {
        _context = context;
        _grades = grades;
        _notifications = notifications;
    }

    public async Task<Result<string>> Handle(GradeResponseCommand request, CancellationToken cancellationToken)
    {
        var response = await _context.TestResponses
            .Include(r => r.Attempt)
                .ThenInclude(a => a.Test)
            .Include(r => r.Question)
            .FirstOrDefaultAsync(r => r.Id == request.ResponseId, cancellationToken);

        if (response is null)
            return Result.Failure<string>("Ответ не найден.");

        if (response.Attempt.Test.CreatedById != request.TeacherId)
            return Result.Failure<string>("Вы не являетесь автором этого теста.");

        if (request.Points < 0 || request.Points > response.Question.Points)
            return Result.Failure<string>($"Баллы должны быть от 0 до {response.Question.Points}.");

        response.Points = request.Points;
        response.IsCorrect = request.Points > 0;
        response.TeacherComment = request.Comment;

        // Recalculate attempt score
        var attempt = response.Attempt;
        var allResponses = await _context.TestResponses
            .Where(r => r.AttemptId == attempt.Id)
            .ToListAsync(cancellationToken);

        attempt.Score = allResponses.Sum(r => r.Points ?? 0);

        // Check if all OpenAnswer responses are graded
        var hasUngraded = allResponses.Any(r => r.IsCorrect == null);
        var statusFlipped = false;
        if (!hasUngraded && attempt.Status == AttemptStatus.NeedsReview)
        {
            attempt.Status = AttemptStatus.Completed;
            statusFlipped = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (!hasUngraded && attempt.Test.CourseId.HasValue)
        {
            await _grades.UpsertAsync(new GradeRecordUpsert(
                attempt.StudentId,
                attempt.Test.CourseId.Value,
                "Test",
                attempt.Id,
                null,
                attempt.Test.Title,
                attempt.Score ?? 0,
                attempt.Test.MaxScore,
                null,
                attempt.CompletedAt ?? DateTime.UtcNow,
                request.TeacherId), cancellationToken);
        }

        if (statusFlipped)
        {
            await _notifications.PublishAsync(new NotificationRequest(
                attempt.StudentId, NotificationType.Grade,
                "Тест проверен",
                $"«{attempt.Test.Title}»: {attempt.Score} баллов",
                $"/student/test/{attempt.TestId}/result/{attempt.Id}"), cancellationToken);
        }

        return Result.Success<string>("Ответ оценён.");
    }
}

using Assignments.Application.Interfaces;
using Assignments.Domain.Enums;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Assignments.Commands.GradeSubmission;

public class GradeSubmissionCommandHandler : IRequestHandler<GradeSubmissionCommand, Result<string>>
{
    private readonly IAssignmentsDbContext _db;
    private readonly IGradeRecordWriter _grades;
    private readonly INotificationDispatcher _notifications;

    public GradeSubmissionCommandHandler(
        IAssignmentsDbContext db,
        IGradeRecordWriter grades,
        INotificationDispatcher notifications)
    {
        _db = db;
        _grades = grades;
        _notifications = notifications;
    }

    public async Task<Result<string>> Handle(GradeSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _db.AssignmentSubmissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == request.SubmissionId, cancellationToken);

        if (submission is null) return Result.Failure<string>("Работа не найдена.");
        if (submission.Assignment.CreatedById != request.TeacherId)
            return Result.Failure<string>("Нет прав на проверку.");

        if (request.Status != SubmissionStatus.Graded && request.Status != SubmissionStatus.ReturnedForRevision)
            return Result.Failure<string>("Недопустимый статус.");

        if (request.Score < 0 || request.Score > submission.Assignment.MaxScore)
            return Result.Failure<string>($"Оценка должна быть от 0 до {submission.Assignment.MaxScore}.");

        submission.Score = request.Score;
        submission.TeacherComment = request.Comment;
        submission.Status = request.Status;
        submission.GradedAt = DateTime.UtcNow;
        submission.GradedById = request.TeacherId;

        await _db.SaveChangesAsync(cancellationToken);

        if (request.Status == SubmissionStatus.Graded && submission.Score.HasValue && submission.Assignment.CourseId.HasValue)
        {
            await _grades.UpsertAsync(new GradeRecordUpsert(
                submission.StudentId,
                submission.Assignment.CourseId.Value,
                "Assignment",
                null,
                submission.Id,
                submission.Assignment.Title,
                submission.Score.Value,
                submission.Assignment.MaxScore,
                request.Comment,
                submission.GradedAt ?? DateTime.UtcNow,
                request.TeacherId), cancellationToken);
        }
        else
        {
            await _grades.DeleteByAssignmentSubmissionAsync(submission.Id, cancellationToken);
        }

        var title = request.Status == SubmissionStatus.Graded ? "Работа оценена" : "Возвращено на доработку";
        var message = request.Status == SubmissionStatus.Graded
            ? $"«{submission.Assignment.Title}»: {request.Score}/{submission.Assignment.MaxScore}"
            : $"«{submission.Assignment.Title}»";
        await _notifications.PublishAsync(new NotificationRequest(
            submission.StudentId, NotificationType.Grade, title, message,
            $"/student/assignment/{submission.AssignmentId}"), cancellationToken);

        return Result.Success(request.Status == SubmissionStatus.Graded
            ? "Оценка выставлена."
            : "Работа возвращена на доработку.");
    }
}

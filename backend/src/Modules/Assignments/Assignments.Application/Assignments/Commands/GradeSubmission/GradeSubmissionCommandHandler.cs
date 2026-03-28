using Assignments.Application.Interfaces;
using Assignments.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Assignments.Commands.GradeSubmission;

public class GradeSubmissionCommandHandler : IRequestHandler<GradeSubmissionCommand, Result<string>>
{
    private readonly IAssignmentsDbContext _db;

    public GradeSubmissionCommandHandler(IAssignmentsDbContext db) => _db = db;

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
        return Result.Success(request.Status == SubmissionStatus.Graded
            ? "Оценка выставлена."
            : "Работа возвращена на доработку.");
    }
}

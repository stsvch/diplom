using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using Grading.Application.DTOs;
using Grading.Application.Interfaces;
using Grading.Domain.Entities;
using MediatR;

namespace Grading.Application.Grades.Commands.CreateGrade;

public class CreateGradeCommandHandler : IRequestHandler<CreateGradeCommand, Result<GradeDto>>
{
    private readonly IGradingDbContext _context;
    private readonly INotificationDispatcher _notifications;

    public CreateGradeCommandHandler(IGradingDbContext context, INotificationDispatcher notifications)
    {
        _context = context;
        _notifications = notifications;
    }

    public async Task<Result<GradeDto>> Handle(CreateGradeCommand request, CancellationToken cancellationToken)
    {
        var grade = new Grade
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            SourceType = request.SourceType,
            TestAttemptId = request.TestAttemptId,
            AssignmentSubmissionId = request.AssignmentSubmissionId,
            Title = request.Title,
            Score = request.Score,
            MaxScore = request.MaxScore,
            Comment = request.Comment,
            GradedAt = request.GradedAt,
            GradedById = request.GradedById
        };

        _context.Grades.Add(grade);
        await _context.SaveChangesAsync(cancellationToken);

        await _notifications.PublishAsync(new NotificationRequest(
            request.StudentId,
            NotificationType.Grade,
            "Оценка выставлена",
            $"{grade.Title}: {grade.Score}/{grade.MaxScore}",
            "/student/grades"), cancellationToken);

        return Result.Success(MapToDto(grade));
    }

    private static GradeDto MapToDto(Grade grade) => new()
    {
        Id = grade.Id,
        StudentId = grade.StudentId,
        CourseId = grade.CourseId,
        SourceType = grade.SourceType,
        Title = grade.Title,
        Score = grade.Score,
        MaxScore = grade.MaxScore,
        Comment = grade.Comment,
        GradedAt = grade.GradedAt
    };
}

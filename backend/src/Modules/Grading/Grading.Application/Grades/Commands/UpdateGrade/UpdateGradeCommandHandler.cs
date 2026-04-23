using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using Grading.Application.DTOs;
using Grading.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Grading.Application.Grades.Commands.UpdateGrade;

public class UpdateGradeCommandHandler : IRequestHandler<UpdateGradeCommand, Result<GradeDto>>
{
    private readonly IGradingDbContext _context;
    private readonly INotificationDispatcher _notifications;

    public UpdateGradeCommandHandler(IGradingDbContext context, INotificationDispatcher notifications)
    {
        _context = context;
        _notifications = notifications;
    }

    public async Task<Result<GradeDto>> Handle(UpdateGradeCommand request, CancellationToken cancellationToken)
    {
        var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (grade is null)
            return Result.Failure<GradeDto>("Grade not found");

        var scoreChanged = grade.Score != request.Score;
        grade.Score = request.Score;
        grade.MaxScore = request.MaxScore;
        grade.Comment = request.Comment;

        await _context.SaveChangesAsync(cancellationToken);

        if (scoreChanged)
        {
            await _notifications.PublishAsync(new NotificationRequest(
                grade.StudentId,
                NotificationType.Grade,
                "Оценка обновлена",
                $"{grade.Title}: {grade.Score}/{grade.MaxScore}",
                "/student/grades"), cancellationToken);
        }

        return Result.Success(new GradeDto
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
        });
    }
}

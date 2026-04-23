using Assignments.Application.DTOs;
using Assignments.Application.Interfaces;
using Assignments.Domain.Entities;
using Assignments.Domain.Enums;
using AutoMapper;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Assignments.Commands.SubmitAssignment;

public class SubmitAssignmentCommandHandler : IRequestHandler<SubmitAssignmentCommand, Result<SubmissionDto>>
{
    private readonly IAssignmentsDbContext _db;
    private readonly IMapper _mapper;
    private readonly INotificationDispatcher _notifications;

    public SubmitAssignmentCommandHandler(IAssignmentsDbContext db, IMapper mapper, INotificationDispatcher notifications)
    {
        _db = db;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<Result<SubmissionDto>> Handle(SubmitAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken);
        if (assignment is null) return Result.Failure<SubmissionDto>("Задание не найдено.");

        if (assignment.Deadline.HasValue && DateTime.UtcNow > assignment.Deadline.Value)
            return Result.Failure<SubmissionDto>("Дедлайн сдачи истёк.");

        var existingSubmissions = await _db.AssignmentSubmissions
            .Where(s => s.AssignmentId == request.AssignmentId && s.StudentId == request.StudentId)
            .ToListAsync(cancellationToken);

        var hasPending = existingSubmissions.Any(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview);
        if (hasPending) return Result.Failure<SubmissionDto>("У вас уже есть работа на проверке.");

        if (assignment.MaxAttempts.HasValue && existingSubmissions.Count >= assignment.MaxAttempts.Value)
            return Result.Failure<SubmissionDto>("Превышено максимальное количество попыток.");

        var submission = new AssignmentSubmission
        {
            AssignmentId = request.AssignmentId,
            StudentId = request.StudentId,
            AttemptNumber = existingSubmissions.Count + 1,
            Content = request.Content,
            SubmittedAt = DateTime.UtcNow,
            Status = SubmissionStatus.Submitted,
        };

        _db.AssignmentSubmissions.Add(submission);
        await _db.SaveChangesAsync(cancellationToken);

        await _notifications.PublishAsync(new NotificationRequest(
            assignment.CreatedById,
            NotificationType.Message,
            "Новая работа на проверку",
            $"Студент отправил «{assignment.Title}»",
            $"/teacher/assignment/{assignment.Id}"), cancellationToken);

        var dto = _mapper.Map<SubmissionDto>(submission);
        dto.MaxScore = assignment.MaxScore;
        return Result.Success(dto);
    }
}

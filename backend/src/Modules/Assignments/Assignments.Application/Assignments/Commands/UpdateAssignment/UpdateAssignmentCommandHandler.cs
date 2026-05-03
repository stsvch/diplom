using Assignments.Application.DTOs;
using Assignments.Application.Interfaces;
using Assignments.Domain.Entities;
using AssignmentCriteriaEntity = Assignments.Domain.Entities.AssignmentCriteria;
using AutoMapper;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Assignments.Commands.UpdateAssignment;

public class UpdateAssignmentCommandHandler : IRequestHandler<UpdateAssignmentCommand, Result<AssignmentDto>>
{
    private readonly IAssignmentsDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationDispatcher _notifications;
    private readonly ICalendarEventPublisher _calendar;
    private readonly IEnrollmentReadService _enrollment;

    public UpdateAssignmentCommandHandler(
        IAssignmentsDbContext context,
        IMapper mapper,
        INotificationDispatcher notifications,
        ICalendarEventPublisher calendar,
        IEnrollmentReadService enrollment)
    {
        _context = context;
        _mapper = mapper;
        _notifications = notifications;
        _calendar = calendar;
        _enrollment = enrollment;
    }

    public async Task<Result<AssignmentDto>> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.Assignments
            .Include(a => a.Submissions)
            .Include(a => a.CriteriaItems)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (assignment is null)
            return Result.Failure<AssignmentDto>("Задание не найдено.");

        if (assignment.CreatedById != request.CreatedById)
            return Result.Failure<AssignmentDto>("Вы не являетесь автором этого задания.");

        var oldDeadline = assignment.Deadline;

        assignment.CourseId = request.CourseId;
        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.Criteria = request.Criteria;
        assignment.Deadline = request.Deadline;
        assignment.MaxAttempts = request.MaxAttempts;
        assignment.MaxScore = request.MaxScore;
        if (request.SubmissionFormat.HasValue)
            assignment.SubmissionFormat = request.SubmissionFormat.Value;

        if (request.CriteriaItems is not null)
        {
            // Полностью заменяем структурированные критерии
            _context.AssignmentCriteria.RemoveRange(assignment.CriteriaItems);
            assignment.CriteriaItems.Clear();

            var order = 0;
            foreach (var c in request.CriteriaItems)
            {
                assignment.CriteriaItems.Add(new AssignmentCriteriaEntity
                {
                    AssignmentId = assignment.Id,
                    Text = c.Text,
                    MaxPoints = c.MaxPoints,
                    OrderIndex = order++
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (oldDeadline != assignment.Deadline)
        {
            await _calendar.DeleteBySourceAsync("Assignment", assignment.Id, cancellationToken);

            var students = await _enrollment.GetActiveStudentIdsAsync(request.CourseId, cancellationToken);
            if (students.Count > 0 && assignment.Deadline.HasValue)
            {
                var deadline = assignment.Deadline.Value;
                var upserts = students.Select(sid => new CalendarEventUpsert(
                    sid, request.CourseId, assignment.Title, null,
                    DateTime.SpecifyKind(deadline.Date, DateTimeKind.Utc),
                    deadline.ToString("HH:mm"),
                    CalendarEventType.Deadline, "Assignment", assignment.Id)).ToList();
                await _calendar.UpsertManyAsync(upserts, cancellationToken);
            }

            if (students.Count > 0)
            {
                var msg = assignment.Deadline.HasValue
                    ? $"«{assignment.Title}» — новый дедлайн {assignment.Deadline:dd.MM.yyyy HH:mm}"
                    : $"«{assignment.Title}» — дедлайн снят";
                var notifications = students.Select(sid => new NotificationRequest(
                    sid, NotificationType.Deadline, "Изменён срок задания", msg,
                    $"/student/assignment/{assignment.Id}")).ToList();
                await _notifications.PublishManyAsync(notifications, cancellationToken);
            }
        }

        var dto = _mapper.Map<AssignmentDto>(assignment);
        return Result.Success(dto);
    }
}

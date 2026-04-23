using Assignments.Application.DTOs;
using Assignments.Application.Interfaces;
using Assignments.Domain.Entities;
using AutoMapper;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;

namespace Assignments.Application.Assignments.Commands.CreateAssignment;

public class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, Result<AssignmentDto>>
{
    private readonly IAssignmentsDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationDispatcher _notifications;
    private readonly ICalendarEventPublisher _calendar;
    private readonly IEnrollmentReadService _enrollment;

    public CreateAssignmentCommandHandler(
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

    public async Task<Result<AssignmentDto>> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = new Assignment
        {
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            Criteria = request.Criteria,
            Deadline = request.Deadline,
            MaxAttempts = request.MaxAttempts,
            MaxScore = request.MaxScore,
            CreatedById = request.CreatedById
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        var students = await _enrollment.GetActiveStudentIdsAsync(request.CourseId, cancellationToken);
        if (students.Count > 0)
        {
            var notifications = students.Select(sid => new NotificationRequest(
                sid,
                NotificationType.Course,
                "Новое задание",
                $"«{assignment.Title}»",
                $"/student/assignment/{assignment.Id}")).ToList();
            await _notifications.PublishManyAsync(notifications, cancellationToken);

            if (assignment.Deadline.HasValue)
            {
                var deadline = assignment.Deadline.Value;
                var upserts = students.Select(sid => new CalendarEventUpsert(
                    sid, request.CourseId, assignment.Title, null,
                    DateTime.SpecifyKind(deadline.Date, DateTimeKind.Utc),
                    deadline.ToString("HH:mm"),
                    CalendarEventType.Deadline, "Assignment", assignment.Id)).ToList();
                await _calendar.UpsertManyAsync(upserts, cancellationToken);
            }
        }

        var dto = _mapper.Map<AssignmentDto>(assignment);
        return Result.Success(dto);
    }
}

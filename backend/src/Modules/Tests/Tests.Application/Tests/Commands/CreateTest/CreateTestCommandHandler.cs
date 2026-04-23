using AutoMapper;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;
using Tests.Domain.Entities;

namespace Tests.Application.Tests.Commands.CreateTest;

public class CreateTestCommandHandler : IRequestHandler<CreateTestCommand, Result<TestDetailDto>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationDispatcher _notifications;
    private readonly ICalendarEventPublisher _calendar;
    private readonly IEnrollmentReadService _enrollment;

    public CreateTestCommandHandler(
        ITestsDbContext context,
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

    public async Task<Result<TestDetailDto>> Handle(CreateTestCommand request, CancellationToken cancellationToken)
    {
        var test = new Test
        {
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            CreatedById = request.CreatedById,
            TimeLimitMinutes = request.TimeLimitMinutes,
            MaxAttempts = request.MaxAttempts,
            Deadline = request.Deadline,
            ShuffleQuestions = request.ShuffleQuestions,
            ShuffleAnswers = request.ShuffleAnswers,
            ShowCorrectAnswers = request.ShowCorrectAnswers,
            MaxScore = 0
        };

        _context.Tests.Add(test);
        await _context.SaveChangesAsync(cancellationToken);

        var students = await _enrollment.GetActiveStudentIdsAsync(request.CourseId, cancellationToken);
        if (students.Count > 0)
        {
            var notifications = students.Select(sid => new NotificationRequest(
                sid, NotificationType.Course, "Новый тест",
                $"«{test.Title}»", $"/student/test/{test.Id}/play")).ToList();
            await _notifications.PublishManyAsync(notifications, cancellationToken);

            if (test.Deadline.HasValue)
            {
                var deadline = test.Deadline.Value;
                var upserts = students.Select(sid => new CalendarEventUpsert(
                    sid, request.CourseId, test.Title, null,
                    DateTime.SpecifyKind(deadline.Date, DateTimeKind.Utc),
                    deadline.ToString("HH:mm"),
                    CalendarEventType.Quiz, "Test", test.Id)).ToList();
                await _calendar.UpsertManyAsync(upserts, cancellationToken);
            }
        }

        var dto = _mapper.Map<TestDetailDto>(test);
        return Result.Success(dto);
    }
}

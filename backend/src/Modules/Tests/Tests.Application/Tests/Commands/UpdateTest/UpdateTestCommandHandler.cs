using AutoMapper;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;

namespace Tests.Application.Tests.Commands.UpdateTest;

public class UpdateTestCommandHandler : IRequestHandler<UpdateTestCommand, Result<TestDetailDto>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationDispatcher _notifications;
    private readonly ICalendarEventPublisher _calendar;
    private readonly IEnrollmentReadService _enrollment;

    public UpdateTestCommandHandler(
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

    public async Task<Result<TestDetailDto>> Handle(UpdateTestCommand request, CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .Include(t => t.Questions)
                .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (test is null)
            return Result.Failure<TestDetailDto>("Тест не найден.");

        if (test.CreatedById != request.CreatedById)
            return Result.Failure<TestDetailDto>("Вы не являетесь автором этого теста.");

        var oldDeadline = test.Deadline;

        test.CourseId = request.CourseId;
        test.Title = request.Title;
        test.Description = request.Description;
        test.TimeLimitMinutes = request.TimeLimitMinutes;
        test.MaxAttempts = request.MaxAttempts;
        test.Deadline = request.Deadline;
        test.ShuffleQuestions = request.ShuffleQuestions;
        test.ShuffleAnswers = request.ShuffleAnswers;
        test.ShowCorrectAnswers = request.ShowCorrectAnswers;

        await _context.SaveChangesAsync(cancellationToken);

        if (oldDeadline != test.Deadline)
        {
            await _calendar.DeleteBySourceAsync("Test", test.Id, cancellationToken);

            var students = await _enrollment.GetActiveStudentIdsAsync(request.CourseId, cancellationToken);
            if (students.Count > 0 && test.Deadline.HasValue)
            {
                var deadline = test.Deadline.Value;
                var upserts = students.Select(sid => new CalendarEventUpsert(
                    sid, request.CourseId, test.Title, null,
                    DateTime.SpecifyKind(deadline.Date, DateTimeKind.Utc),
                    deadline.ToString("HH:mm"),
                    CalendarEventType.Quiz, "Test", test.Id)).ToList();
                await _calendar.UpsertManyAsync(upserts, cancellationToken);
            }

            if (students.Count > 0)
            {
                var msg = test.Deadline.HasValue
                    ? $"«{test.Title}» — новый дедлайн {test.Deadline:dd.MM.yyyy HH:mm}"
                    : $"«{test.Title}» — дедлайн снят";
                var notifications = students.Select(sid => new NotificationRequest(
                    sid, NotificationType.Deadline, "Изменён срок теста", msg,
                    $"/student/test/{test.Id}/play")).ToList();
                await _notifications.PublishManyAsync(notifications, cancellationToken);
            }
        }

        var dto = _mapper.Map<TestDetailDto>(test);
        return Result.Success(dto);
    }
}

using Assignments.Infrastructure.Persistence;
using Content.Infrastructure.Persistence;
using Courses.Infrastructure.Persistence;
using EduPlatform.Shared.Application.Contracts;
using Grading.Infrastructure.Persistence;
using Messaging.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Notifications.Infrastructure.Persistence;
using Progress.Infrastructure.Persistence;
using Scheduling.Infrastructure.Persistence;
using Tests.Infrastructure.Persistence;

namespace EduPlatform.Host.Services;

public class UserDeletionGuard : IUserDeletionGuard
{
    private readonly CoursesDbContext _coursesDbContext;
    private readonly AssignmentsDbContext _assignmentsDbContext;
    private readonly TestsDbContext _testsDbContext;
    private readonly SchedulingDbContext _schedulingDbContext;
    private readonly NotificationsDbContext _notificationsDbContext;
    private readonly ProgressDbContext _progressDbContext;
    private readonly GradingDbContext _gradingDbContext;
    private readonly ContentDbContext _contentDbContext;
    private readonly IMongoDatabase _mongoDatabase;

    public UserDeletionGuard(
        CoursesDbContext coursesDbContext,
        AssignmentsDbContext assignmentsDbContext,
        TestsDbContext testsDbContext,
        SchedulingDbContext schedulingDbContext,
        NotificationsDbContext notificationsDbContext,
        ProgressDbContext progressDbContext,
        GradingDbContext gradingDbContext,
        ContentDbContext contentDbContext,
        IMongoDatabase mongoDatabase)
    {
        _coursesDbContext = coursesDbContext;
        _assignmentsDbContext = assignmentsDbContext;
        _testsDbContext = testsDbContext;
        _schedulingDbContext = schedulingDbContext;
        _notificationsDbContext = notificationsDbContext;
        _progressDbContext = progressDbContext;
        _gradingDbContext = gradingDbContext;
        _contentDbContext = contentDbContext;
        _mongoDatabase = mongoDatabase;
    }

    public async Task<UserDeletionCheckResult> CheckAsync(string userId, CancellationToken cancellationToken = default)
    {
        var reasons = new List<string>();

        if (await _coursesDbContext.Courses.AnyAsync(c => c.TeacherId == userId, cancellationToken))
            reasons.Add("пользователь назначен преподавателем хотя бы в одном курсе");

        if (await _coursesDbContext.CourseEnrollments.AnyAsync(e => e.StudentId == userId, cancellationToken))
            reasons.Add("у пользователя есть записи на курсы");

        if (await _assignmentsDbContext.Assignments.AnyAsync(a => a.CreatedById == userId, cancellationToken))
            reasons.Add("пользователь создал задания");

        if (await _assignmentsDbContext.AssignmentSubmissions.AnyAsync(s => s.StudentId == userId, cancellationToken))
            reasons.Add("у пользователя есть отправки заданий");

        if (await _testsDbContext.Tests.AnyAsync(t => t.CreatedById == userId, cancellationToken))
            reasons.Add("пользователь создал тесты");

        if (await _testsDbContext.TestAttempts.AnyAsync(a => a.StudentId == userId, cancellationToken))
            reasons.Add("у пользователя есть попытки тестов");

        if (await _schedulingDbContext.ScheduleSlots.AnyAsync(s => s.TeacherId == userId, cancellationToken))
            reasons.Add("у пользователя есть слоты расписания преподавателя");

        if (await _schedulingDbContext.SessionBookings.AnyAsync(b => b.StudentId == userId, cancellationToken))
            reasons.Add("у пользователя есть бронирования занятий");

        if (await _notificationsDbContext.Notifications.AnyAsync(n => n.UserId == userId, cancellationToken))
            reasons.Add("у пользователя есть уведомления");

        if (await _progressDbContext.LessonProgresses.AnyAsync(p => p.StudentId == userId, cancellationToken))
            reasons.Add("у пользователя есть прогресс по урокам");

        if (await _gradingDbContext.Grades.AnyAsync(
                g => g.StudentId == userId || g.GradedById == userId,
                cancellationToken))
        {
            reasons.Add("у пользователя есть записи об оценивании");
        }

        if (await _contentDbContext.Attachments.AnyAsync(a => a.UploadedById == userId, cancellationToken))
            reasons.Add("пользователь загружал файлы в контент");

        var chats = _mongoDatabase.GetCollection<ChatDocument>("chats");
        var messages = _mongoDatabase.GetCollection<MessageDocument>("messages");

        var chatFilter = Builders<ChatDocument>.Filter.Or(
            Builders<ChatDocument>.Filter.AnyEq(c => c.ParticipantIds, userId),
            Builders<ChatDocument>.Filter.Eq(c => c.OwnerId, userId));

        if (await chats.Find(chatFilter).AnyAsync(cancellationToken))
            reasons.Add("пользователь участвует в чатах");

        var messageFilter = Builders<MessageDocument>.Filter.Eq(m => m.SenderId, userId);
        if (await messages.Find(messageFilter).AnyAsync(cancellationToken))
            reasons.Add("пользователь отправлял сообщения");

        return new UserDeletionCheckResult(reasons.Count == 0, reasons);
    }
}

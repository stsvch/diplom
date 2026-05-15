// Файл: SubscriptionGracePeriodWorker.cs
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Payments.Application.Interfaces;
using Payments.Domain.Enums;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace EduPlatform.Host.Services;

// Фоновый сервис SubscriptionGracePeriodWorker выполняет периодическую задачу вне HTTP-запроса.
/// <summary>
/// Освобождает будущие брони студента, чья подписка дольше grace-периода
/// находится в PastDue/Unpaid. Работает раз в 15 минут.
/// </summary>
public class SubscriptionGracePeriodWorker : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionGracePeriodWorker> _logger;
    private readonly TimeSpan _gracePeriod;

    public SubscriptionGracePeriodWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionGracePeriodWorker> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        var days = configuration.GetValue<int?>("Subscription:GracePeriodDays") ?? 7;
        _gracePeriod = TimeSpan.FromDays(Math.Max(1, days));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (TaskCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscription grace period iteration failed");
            }

            try { await Task.Delay(RunInterval, stoppingToken); }
            catch (TaskCanceledException) { return; }
        }
    }

    private async Task ProcessOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var paymentsDb = scope.ServiceProvider.GetRequiredService<IPaymentsDbContext>();
        var schedulingDb = scope.ServiceProvider.GetRequiredService<ISchedulingDbContext>();
        var entitlements = scope.ServiceProvider.GetRequiredService<ISubscriptionEntitlementProvider>();
        var calendar = scope.ServiceProvider.GetRequiredService<ICalendarEventPublisher>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();

        var threshold = DateTime.UtcNow - _gracePeriod;

        var expiredUserIds = await paymentsDb.UserSubscriptions
            .Where(s => (s.Status == UserSubscriptionStatus.PastDue
                      || s.Status == UserSubscriptionStatus.Unpaid)
                     && s.PastDueSinceUtc != null
                     && s.PastDueSinceUtc <= threshold)
            .Select(s => s.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (expiredUserIds.Count == 0) return;

        var now = DateTime.UtcNow;
        var graceDays = (int)_gracePeriod.TotalDays;

        foreach (var userId in expiredUserIds)
        {
            var futureBookings = await schedulingDb.SessionBookings
                .Include(b => b.Slot)
                .Where(b => b.StudentId == userId
                         && b.Status == BookingStatus.Booked
                         && b.StartTime > now)
                .ToListAsync(cancellationToken);

            if (futureBookings.Count == 0) continue;

            // Сразу подгружаем брони слотов, которые мы трогаем, — нужно для решения,
            // удалять ли у учителя событие из календаря (если других записей не осталось).
            var affectedSlotIds = futureBookings.Select(b => b.SlotId).Distinct().ToList();
            var allBookingsOfAffectedSlots = await schedulingDb.SessionBookings
                .Where(b => affectedSlotIds.Contains(b.SlotId))
                .Select(b => new { b.Id, b.SlotId, b.Status })
                .ToListAsync(cancellationToken);

            foreach (var booking in futureBookings)
            {
                booking.Status = BookingStatus.Cancelled;
                if (booking.Slot.Status == SlotStatus.Full)
                    booking.Slot.Status = SlotStatus.Available;
            }

            await schedulingDb.SaveChangesAsync(cancellationToken);

            var cancelledIds = futureBookings.Select(b => b.Id).ToHashSet();

            foreach (var booking in futureBookings)
            {
                await entitlements.RefundUsageAsync(booking.Id, cancellationToken);
                await calendar.DeleteBySourceForUserAsync("ScheduleSlot", booking.SlotId, userId, cancellationToken);

                var slotStillHasActiveBookings = allBookingsOfAffectedSlots.Any(x =>
                    x.SlotId == booking.SlotId
                    && !cancelledIds.Contains(x.Id)
                    && x.Status == BookingStatus.Booked);
                if (!slotStillHasActiveBookings)
                {
                    await calendar.DeleteBySourceForUserAsync(
                        "ScheduleSlot", booking.SlotId, booking.Slot.TeacherId, cancellationToken);
                }

                await notifications.PublishAsync(new NotificationRequest(
                    userId,
                    NotificationType.Course,
                    "Бронь отменена",
                    $"«{booking.Slot.Title}» отменено: подписка не оплачена дольше {graceDays} дн.",
                    "/student/schedule"), cancellationToken);

                await notifications.PublishAsync(new NotificationRequest(
                    booking.Slot.TeacherId,
                    NotificationType.Course,
                    "Запись освобождена",
                    $"{booking.StudentName} потерял подписку — слот «{booking.Slot.Title}» снова свободен.",
                    "/teacher/schedule"), cancellationToken);
            }

            _logger.LogInformation(
                "Released {Count} bookings for user {UserId} after subscription grace period",
                futureBookings.Count, userId);
        }
    }
}

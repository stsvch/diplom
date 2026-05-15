// CancelBookingCommandHandler.cs

using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Commands.CancelBooking;

/// <summary>
/// Обработчик CQRS-команды CancelBookingCommand: выполняет сценарий изменения состояния и сохраняет результат.
/// </summary>
public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result<string>>
{
    private const int LateCancelWindowHours = 24;

    private readonly ISchedulingDbContext _context;
    private readonly ICalendarEventPublisher _calendar;
    private readonly INotificationDispatcher _notifications;
    private readonly ISubscriptionEntitlementProvider _entitlements;

    public CancelBookingCommandHandler(
        ISchedulingDbContext context,
        ICalendarEventPublisher calendar,
        INotificationDispatcher notifications,
        ISubscriptionEntitlementProvider entitlements)
    {
        _context = context;
        _calendar = calendar;
        _notifications = notifications;
        _entitlements = entitlements;
    }

    // Основной сценарий handler-а: проверки, чтение/изменение данных и возврат результата.
    public async Task<Result<string>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var slot = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.SlotId, cancellationToken);

        if (slot == null)
            return Result.Failure<string>("Слот не найден.");

        var booking = slot.Bookings.FirstOrDefault(b => b.StudentId == request.StudentId && b.Status == BookingStatus.Booked);

        if (booking == null)
            return Result.Failure<string>("Запись не найдена.");

        var hoursToStart = (slot.StartTime - DateTime.UtcNow).TotalHours;
        var inGracePeriod = hoursToStart >= LateCancelWindowHours;

        booking.Status = inGracePeriod ? BookingStatus.Cancelled : BookingStatus.LateCancelled;

        if (slot.Status == SlotStatus.Full)
            slot.Status = SlotStatus.Available;

        await _context.SaveChangesAsync(cancellationToken);

        if (inGracePeriod)
            await _entitlements.RefundUsageAsync(booking.Id, cancellationToken);

        await _calendar.DeleteBySourceForUserAsync("ScheduleSlot", slot.Id, request.StudentId, cancellationToken);

        await _notifications.PublishAsync(new NotificationRequest(
            slot.TeacherId, NotificationType.Message,
            inGracePeriod ? "Запись отменена" : "Запись отменена с опозданием",
            $"Студент отменил запись на «{slot.Title}»",
            "/teacher/schedule"), cancellationToken);

        return Result.Success(inGracePeriod
            ? "Запись отменена, занятие возвращено в квоту."
            : "Запись отменена. По правилам платформы занятие списано (отмена менее чем за 24 часа).");
    }
}

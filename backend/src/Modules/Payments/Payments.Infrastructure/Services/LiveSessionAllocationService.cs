using EduPlatform.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Payments.Application.Interfaces;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Infrastructure.Configuration;

namespace Payments.Infrastructure.Services;

/// <summary>
/// Распределяет часть оплаченной подписки между учителями пропорционально
/// числу проведённых живых занятий студента в течение периода подписки.
/// </summary>
public class LiveSessionAllocationService
{
    public const string StrategyName = "LiveSessionsCompletedV1";

    private readonly IPaymentsDbContext _context;
    private readonly ICompletedBookingReadService _bookingReadService;
    private readonly PaymentsOptions _paymentsOptions;

    public LiveSessionAllocationService(
        IPaymentsDbContext context,
        ICompletedBookingReadService bookingReadService,
        IOptions<PaymentsOptions> paymentsOptions)
    {
        _context = context;
        _bookingReadService = bookingReadService;
        _paymentsOptions = paymentsOptions.Value;
    }

    /// <summary>
    /// Создаёт <see cref="SubscriptionAllocationRun"/> для live-session стратегии за указанный период.
    /// Идемпотентно: если Run для этого invoice + стратегии уже есть — возвращает его.
    /// </summary>
    public async Task<SubscriptionAllocationRun?> AllocateForPeriodAsync(
        Guid subscriptionInvoiceId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.SubscriptionAllocationRuns
            .FirstOrDefaultAsync(r => r.SubscriptionInvoiceId == subscriptionInvoiceId
                                   && r.Strategy == StrategyName, cancellationToken);
        if (existing != null)
            return existing;

        var invoice = await _context.SubscriptionInvoices
            .FirstOrDefaultAsync(i => i.Id == subscriptionInvoiceId, cancellationToken);
        if (invoice == null)
            return null;

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.Id == invoice.UserSubscriptionId, cancellationToken);
        if (subscription == null)
            return null;

        var completedBookings = await _bookingReadService.GetCompletedBetweenAsync(
            subscription.UserId, periodStart, periodEnd, cancellationToken);

        if (completedBookings.Count == 0)
            return null;

        var bookingIds = completedBookings.Select(b => b.BookingId).ToHashSet();

        var consumedUsages = await _context.SubscriptionUsages
            .Where(u => u.UserSubscriptionId == subscription.Id
                     && !u.IsRefunded
                     && bookingIds.Contains(u.SourceBookingId))
            .Select(u => u.SourceBookingId)
            .ToListAsync(cancellationToken);

        var consumed = consumedUsages.ToHashSet();
        var eligible = completedBookings.Where(b => consumed.Contains(b.BookingId)).ToList();

        if (eligible.Count == 0)
            return null;

        var grossAmount = invoice.AmountPaid;
        var commissionPercent = Math.Clamp(_paymentsOptions.PlatformCommissionPercent, 0m, 100m);
        var platformCommission = decimal.Round(grossAmount * commissionPercent / 100m, 2, MidpointRounding.AwayFromZero);
        var netAmount = grossAmount - platformCommission;

        var run = new SubscriptionAllocationRun
        {
            SubscriptionInvoiceId = subscriptionInvoiceId,
            UserSubscriptionId = subscription.Id,
            SubscriptionPlanId = subscription.SubscriptionPlanId,
            UserId = subscription.UserId,
            PlanName = subscription.PlanName,
            GrossAmount = grossAmount,
            PlatformCommissionAmount = platformCommission,
            ProviderFeeAmount = 0m,
            NetAmount = netAmount,
            Currency = subscription.Currency,
            Strategy = StrategyName,
            Status = SubscriptionAllocationRunStatus.Applied,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            AllocatedAt = DateTime.UtcNow
        };

        _context.SubscriptionAllocationRuns.Add(run);

        var availableAt = DateTime.UtcNow.AddDays(_paymentsOptions.SettlementHoldDays);

        var perTeacher = eligible
            .GroupBy(b => new { b.TeacherId, b.TeacherName })
            .Select(g => new
            {
                g.Key.TeacherId,
                g.Key.TeacherName,
                Count = g.Count(),
                Bookings = g.Select(x => x.BookingId).ToList()
            })
            .ToList();

        var totalSessions = perTeacher.Sum(t => t.Count);
        run.TeacherCount = perTeacher.Count;
        run.CourseCount = 0;

        decimal allocated = 0m;
        for (var i = 0; i < perTeacher.Count; i++)
        {
            var teacher = perTeacher[i];
            var weight = (decimal)teacher.Count / totalSessions;
            decimal teacherNet;
            if (i == perTeacher.Count - 1)
                teacherNet = netAmount - allocated; // последнему — остаток (на случай округления)
            else
                teacherNet = decimal.Round(netAmount * weight, 2, MidpointRounding.AwayFromZero);
            allocated += teacherNet;

            // Одна строка на учителя (агрегированная по всем его сессиям этого Run-а),
            // BookingId — последняя бронь, чтобы попасть в уникальный индекс.
            // Если хочется разбить по бронированиям — можно сделать line per-booking.
            var line = new SubscriptionAllocationLine
            {
                SubscriptionAllocationRunId = run.Id,
                SubscriptionInvoiceId = subscriptionInvoiceId,
                SubscriptionPlanId = subscription.SubscriptionPlanId,
                UserId = subscription.UserId,
                TeacherId = teacher.TeacherId,
                TeacherName = teacher.TeacherName,
                Source = SubscriptionAllocationSource.LiveSession,
                CourseId = Guid.Empty,
                CourseTitle = string.Empty,
                BookingId = teacher.Bookings.Last(),
                CompletedSessionsCount = teacher.Count,
                AllocationWeight = weight,
                ProgressPercent = 0m,
                TotalLessons = 0,
                CompletedLessons = teacher.Count,
                GrossAmount = decimal.Round(grossAmount * weight, 2, MidpointRounding.AwayFromZero),
                PlatformCommissionAmount = decimal.Round(platformCommission * weight, 2, MidpointRounding.AwayFromZero),
                ProviderFeeAmount = 0m,
                NetAmount = teacherNet,
                Currency = subscription.Currency,
                AvailableAt = availableAt,
                AllocatedAt = DateTime.UtcNow
            };

            _context.SubscriptionAllocationLines.Add(line);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return run;
    }
}

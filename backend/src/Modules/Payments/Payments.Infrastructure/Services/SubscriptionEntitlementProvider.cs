// SubscriptionEntitlementProvider.cs
using EduPlatform.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Payments.Application.Interfaces;
using Payments.Domain.Entities;
using Payments.Domain.Enums;

namespace Payments.Infrastructure.Services;

// Основной тип файла описывает часть модуля и его публичный контракт.
public class SubscriptionEntitlementProvider : ISubscriptionEntitlementProvider
{
    private readonly IPaymentsDbContext _context;

    public SubscriptionEntitlementProvider(IPaymentsDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntitlements> GetForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.UserSubscriptions
            .Where(s => s.UserId == userId && s.Status == UserSubscriptionStatus.Active)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
        {
            return new UserEntitlements(
                HasActiveSubscription: false,
                PlanName: null,
                IndividualSlotsPerMonth: 0,
                IndividualSlotsUsed: 0,
                GroupSlotsPerMonth: 0,
                GroupSlotsUsed: 0,
                CurrentPeriodStart: null,
                CurrentPeriodEnd: null);
        }

        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == subscription.SubscriptionPlanId, cancellationToken);

        var periodStart = subscription.CurrentPeriodStart ?? subscription.StartedAt;

        var usedByType = await _context.SubscriptionUsages
            .Where(u => u.UserSubscriptionId == subscription.Id
                     && u.PeriodStart == periodStart
                     && !u.IsRefunded)
            .GroupBy(u => u.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var individualUsed = usedByType.FirstOrDefault(x => x.Type == SlotUsageType.Individual)?.Count ?? 0;
        var groupUsed = usedByType.FirstOrDefault(x => x.Type == SlotUsageType.Group)?.Count ?? 0;

        return new UserEntitlements(
            HasActiveSubscription: true,
            PlanName: subscription.PlanName,
            IndividualSlotsPerMonth: plan?.IndividualSlotsPerMonth ?? 0,
            IndividualSlotsUsed: individualUsed,
            GroupSlotsPerMonth: plan?.GroupSlotsPerMonth ?? 0,
            GroupSlotsUsed: groupUsed,
            CurrentPeriodStart: subscription.CurrentPeriodStart,
            CurrentPeriodEnd: subscription.CurrentPeriodEnd);
    }

    public async Task RecordUsageAsync(SlotUsageRecord record, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.UserSubscriptions
            .Where(s => s.UserId == record.UserId && s.Status == UserSubscriptionStatus.Active)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"No active subscription for user {record.UserId}.");

        var periodStart = subscription.CurrentPeriodStart ?? subscription.StartedAt;

        var usage = new SubscriptionUsage
        {
            UserSubscriptionId = subscription.Id,
            UserId = record.UserId,
            Type = record.Kind == LiveSlotKind.Individual ? SlotUsageType.Individual : SlotUsageType.Group,
            PeriodStart = periodStart,
            SourceBookingId = record.BookingId,
            UsedAt = DateTime.UtcNow,
            IsRefunded = false
        };

        _context.SubscriptionUsages.Add(usage);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RefundUsageAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var usage = await _context.SubscriptionUsages
            .FirstOrDefaultAsync(u => u.SourceBookingId == bookingId && !u.IsRefunded, cancellationToken);

        if (usage == null)
            return;

        usage.IsRefunded = true;
        usage.RefundedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}

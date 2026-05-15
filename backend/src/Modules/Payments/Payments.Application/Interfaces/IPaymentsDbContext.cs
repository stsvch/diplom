// IPaymentsDbContext.cs
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;

namespace Payments.Application.Interfaces;

// Контракт application-слоя отделяет бизнес-сценарии от конкретной инфраструктуры.
public interface IPaymentsDbContext
{
    DbSet<TeacherPayoutAccount> TeacherPayoutAccounts { get; }
    DbSet<TeacherSettlement> TeacherSettlements { get; }
    DbSet<PayoutRecord> PayoutRecords { get; }
    DbSet<UserPaymentProfile> UserPaymentProfiles { get; }
    DbSet<PaymentAttempt> PaymentAttempts { get; }
    DbSet<CoursePurchase> CoursePurchases { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<UserSubscription> UserSubscriptions { get; }
    DbSet<SubscriptionPaymentAttempt> SubscriptionPaymentAttempts { get; }
    DbSet<SubscriptionInvoice> SubscriptionInvoices { get; }
    DbSet<SubscriptionUsage> SubscriptionUsages { get; }
    DbSet<PaymentMethodRef> PaymentMethods { get; }
    DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

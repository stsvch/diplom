using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;

namespace Payments.Application.Interfaces;

public interface IPaymentsDbContext
{
    DbSet<TeacherPayoutAccount> TeacherPayoutAccounts { get; }
    DbSet<TeacherSettlement> TeacherSettlements { get; }
    DbSet<PayoutRecord> PayoutRecords { get; }
    DbSet<RefundRecord> RefundRecords { get; }
    DbSet<DisputeRecord> DisputeRecords { get; }
    DbSet<UserPaymentProfile> UserPaymentProfiles { get; }
    DbSet<PaymentAttempt> PaymentAttempts { get; }
    DbSet<CoursePurchase> CoursePurchases { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<UserSubscription> UserSubscriptions { get; }
    DbSet<SubscriptionPaymentAttempt> SubscriptionPaymentAttempts { get; }
    DbSet<SubscriptionInvoice> SubscriptionInvoices { get; }
    DbSet<SubscriptionAllocationRun> SubscriptionAllocationRuns { get; }
    DbSet<SubscriptionAllocationLine> SubscriptionAllocationLines { get; }
    DbSet<PaymentMethodRef> PaymentMethods { get; }
    DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

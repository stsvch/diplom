// PaymentsDbContext.cs
using EduPlatform.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Payments.Application.Interfaces;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Persistence;

// Основной тип файла описывает часть модуля и его публичный контракт.
public class PaymentsDbContext : BaseDbContext, IPaymentsDbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<TeacherPayoutAccount> TeacherPayoutAccounts => Set<TeacherPayoutAccount>();
    public DbSet<TeacherSettlement> TeacherSettlements => Set<TeacherSettlement>();
    public DbSet<PayoutRecord> PayoutRecords => Set<PayoutRecord>();
    public DbSet<UserPaymentProfile> UserPaymentProfiles => Set<UserPaymentProfile>();
    public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();
    public DbSet<CoursePurchase> CoursePurchases => Set<CoursePurchase>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<SubscriptionPaymentAttempt> SubscriptionPaymentAttempts => Set<SubscriptionPaymentAttempt>();
    public DbSet<SubscriptionInvoice> SubscriptionInvoices => Set<SubscriptionInvoice>();
    public DbSet<SubscriptionUsage> SubscriptionUsages => Set<SubscriptionUsage>();
    public DbSet<PaymentMethodRef> PaymentMethods => Set<PaymentMethodRef>();
    public DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents => Set<ProcessedWebhookEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("payments");

        modelBuilder.Entity<TeacherPayoutAccount>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TeacherId).IsRequired().HasMaxLength(450);
            e.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            e.Property(x => x.ProviderAccountId).IsRequired().HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.RequirementsSummary).HasMaxLength(4000);
            e.HasIndex(x => x.TeacherId).IsUnique();
            e.HasIndex(x => x.ProviderAccountId).IsUnique();
        });

        modelBuilder.Entity<TeacherSettlement>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TeacherId).IsRequired().HasMaxLength(450);
            e.Property(x => x.CourseTitle).IsRequired().HasMaxLength(500);
            e.Property(x => x.StudentId).IsRequired().HasMaxLength(450);
            e.Property(x => x.StudentName).IsRequired().HasMaxLength(300);
            e.Property(x => x.GrossAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.ProviderFeeAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.PlatformCommissionAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.NetAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).IsRequired().HasMaxLength(16);
            e.Property(x => x.PayoutRecordId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.HasIndex(x => new { x.TeacherId, x.AvailableAt });
            e.HasIndex(x => x.PaymentAttemptId).IsUnique();
            e.HasIndex(x => x.CoursePurchaseId).IsUnique();
            e.HasIndex(x => x.PayoutRecordId);
        });

        modelBuilder.Entity<PayoutRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TeacherId).IsRequired().HasMaxLength(450);
            e.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            e.Property(x => x.ProviderAccountId).HasMaxLength(200);
            e.Property(x => x.ProviderTransferId).HasMaxLength(200);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).IsRequired().HasMaxLength(16);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.FailureMessage).HasMaxLength(2000);
            e.HasIndex(x => new { x.TeacherId, x.RequestedAt });
            e.HasIndex(x => x.ProviderTransferId);
        });

        modelBuilder.Entity<UserPaymentProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).IsRequired().HasMaxLength(450);
            e.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            e.Property(x => x.ProviderCustomerId).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasIndex(x => x.ProviderCustomerId).IsUnique();
        });

        modelBuilder.Entity<PaymentAttempt>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CourseTitle).IsRequired().HasMaxLength(500);
            e.Property(x => x.TeacherId).IsRequired().HasMaxLength(450);
            e.Property(x => x.StudentId).IsRequired().HasMaxLength(450);
            e.Property(x => x.StudentName).IsRequired().HasMaxLength(300);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).IsRequired().HasMaxLength(16);
            e.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.ProviderCustomerId).HasMaxLength(200);
            e.Property(x => x.ProviderSessionId).HasMaxLength(200);
            e.Property(x => x.ProviderPaymentIntentId).HasMaxLength(200);
            e.Property(x => x.ProviderChargeId).HasMaxLength(200);
            e.Property(x => x.ProviderPaymentMethodId).HasMaxLength(200);
            e.Property(x => x.FailureCode).HasMaxLength(200);
            e.Property(x => x.FailureMessage).HasMaxLength(2000);
            e.HasIndex(x => new { x.StudentId, x.CreatedAt });
            e.HasIndex(x => x.ProviderSessionId);
            e.HasIndex(x => x.ProviderPaymentIntentId);
            e.HasIndex(x => x.ProviderChargeId);
        });

        modelBuilder.Entity<CoursePurchase>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CourseTitle).IsRequired().HasMaxLength(500);
            e.Property(x => x.TeacherId).IsRequired().HasMaxLength(450);
            e.Property(x => x.StudentId).IsRequired().HasMaxLength(450);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).IsRequired().HasMaxLength(16);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.HasIndex(x => x.PaymentAttemptId).IsUnique();
            e.HasIndex(x => new { x.StudentId, x.CourseId });
        });

        modelBuilder.Entity<SubscriptionPlan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).IsRequired().HasMaxLength(16);
            e.Property(x => x.BillingInterval).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.ProviderProductId).HasMaxLength(200);
            e.Property(x => x.ProviderPriceId).HasMaxLength(200);
            e.HasIndex(x => new { x.IsActive, x.SortOrder });
            e.HasIndex(x => x.ProviderPriceId);
        });

        modelBuilder.Entity<UserSubscription>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).IsRequired().HasMaxLength(450);
            e.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            e.Property(x => x.ProviderCustomerId).IsRequired().HasMaxLength(200);
            e.Property(x => x.ProviderSubscriptionId).IsRequired().HasMaxLength(200);
            e.Property(x => x.PlanName).IsRequired().HasMaxLength(200);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).IsRequired().HasMaxLength(16);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.HasIndex(x => x.ProviderSubscriptionId).IsUnique();
            e.HasIndex(x => new { x.UserId, x.Status });
            e.HasIndex(x => new { x.UserId, x.SubscriptionPlanId });
        });

        modelBuilder.Entity<SubscriptionPaymentAttempt>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).IsRequired().HasMaxLength(450);
            e.Property(x => x.PlanName).IsRequired().HasMaxLength(200);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).IsRequired().HasMaxLength(16);
            e.Property(x => x.BillingInterval).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.ProviderCustomerId).HasMaxLength(200);
            e.Property(x => x.ProviderSessionId).HasMaxLength(200);
            e.Property(x => x.ProviderSubscriptionId).HasMaxLength(200);
            e.Property(x => x.FailureMessage).HasMaxLength(2000);
            e.HasIndex(x => new { x.UserId, x.CreatedAt });
            e.HasIndex(x => x.ProviderSessionId);
            e.HasIndex(x => x.ProviderSubscriptionId);
        });

        modelBuilder.Entity<SubscriptionInvoice>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).IsRequired().HasMaxLength(450);
            e.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            e.Property(x => x.ProviderInvoiceId).IsRequired().HasMaxLength(200);
            e.Property(x => x.ProviderSubscriptionId).HasMaxLength(200);
            e.Property(x => x.PlanName).IsRequired().HasMaxLength(200);
            e.Property(x => x.AmountDue).HasColumnType("decimal(18,2)");
            e.Property(x => x.AmountPaid).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).IsRequired().HasMaxLength(16);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.BillingReason).HasMaxLength(100);
            e.Property(x => x.FailureMessage).HasMaxLength(2000);
            e.HasIndex(x => x.ProviderInvoiceId).IsUnique();
            e.HasIndex(x => new { x.UserId, x.CreatedAt });
            e.HasIndex(x => x.ProviderSubscriptionId);
        });

        modelBuilder.Entity<SubscriptionUsage>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).IsRequired().HasMaxLength(450);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
            e.HasIndex(x => new { x.UserSubscriptionId, x.PeriodStart, x.Type });
            e.HasIndex(x => x.SourceBookingId).IsUnique();
            e.HasIndex(x => new { x.UserId, x.PeriodStart });
        });

        modelBuilder.Entity<PaymentMethodRef>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).IsRequired().HasMaxLength(450);
            e.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            e.Property(x => x.ProviderCustomerId).IsRequired().HasMaxLength(200);
            e.Property(x => x.ProviderPaymentMethodId).IsRequired().HasMaxLength(200);
            e.Property(x => x.Brand).HasMaxLength(50);
            e.Property(x => x.Last4).HasMaxLength(4);
            e.HasIndex(x => x.ProviderPaymentMethodId).IsUnique();
            e.HasIndex(x => new { x.UserId, x.IsDefault });
        });

        modelBuilder.Entity<ProcessedWebhookEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            e.Property(x => x.ProviderEventId).IsRequired().HasMaxLength(200);
            e.Property(x => x.EventType).IsRequired().HasMaxLength(200);
            e.HasIndex(x => new { x.Provider, x.ProviderEventId }).IsUnique();
        });
    }
}

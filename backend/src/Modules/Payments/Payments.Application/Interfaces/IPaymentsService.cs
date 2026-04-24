using Payments.Application.DTOs;
using EduPlatform.Shared.Application.Models;

namespace Payments.Application.Interfaces;

public interface IPaymentsService
{
    Task<TeacherPayoutAccountDto> GetTeacherPayoutAccountAsync(
        string teacherId,
        CancellationToken cancellationToken = default);

    Task<string> CreateTeacherOnboardingLinkAsync(
        string teacherId,
        string teacherEmail,
        string teacherName,
        CancellationToken cancellationToken = default);

    Task<string> CreateTeacherDashboardLinkAsync(
        string teacherId,
        CancellationToken cancellationToken = default);

    Task<TeacherSettlementSummaryDto> GetTeacherSettlementSummaryAsync(
        string teacherId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeacherSettlementDto>> GetTeacherSettlementsAsync(
        string teacherId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeacherSubscriptionAllocationDto>> GetTeacherSubscriptionAllocationsAsync(
        string teacherId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PayoutRecordDto>> GetTeacherPayoutRecordsAsync(
        string teacherId,
        CancellationToken cancellationToken = default);

    Task<PayoutRecordDto> RequestTeacherPayoutAsync(
        string teacherId,
        CancellationToken cancellationToken = default);

    Task<CourseCheckoutSessionDto> CreateCourseCheckoutAsync(
        Guid courseId,
        string studentId,
        string studentEmail,
        string studentName,
        bool savePaymentMethod,
        CancellationToken cancellationToken = default);

    Task<SubscriptionCheckoutSessionDto> CreateSubscriptionCheckoutAsync(
        Guid subscriptionPlanId,
        string studentId,
        string studentEmail,
        string studentName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentAttemptDto>> GetMyPaymentHistoryAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSubscriptionDto>> GetMySubscriptionsAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubscriptionPaymentAttemptDto>> GetMySubscriptionHistoryAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubscriptionInvoiceDto>> GetMySubscriptionInvoicesAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    Task<SubscriptionPaymentAttemptDto?> GetSubscriptionPaymentAttemptAsync(
        Guid subscriptionPaymentAttemptId,
        string studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CoursePurchaseDto>> GetMyPurchasesAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefundRecordDto>> GetMyRefundsAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DisputeRecordDto>> GetMyDisputesAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DisputeRecordDto>> GetTeacherDisputesAsync(
        string teacherId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubscriptionPlanDto>> GetActiveSubscriptionPlansAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubscriptionPlanDto>> GetAdminSubscriptionPlansAsync(
        CancellationToken cancellationToken = default);

    Task<SubscriptionPlanDto> CreateSubscriptionPlanAsync(
        string name,
        string? description,
        decimal price,
        string currency,
        string billingInterval,
        int billingIntervalCount,
        bool isActive,
        bool isFeatured,
        int sortOrder,
        string? providerProductId,
        string? providerPriceId,
        CancellationToken cancellationToken = default);

    Task<SubscriptionPlanDto> UpdateSubscriptionPlanAsync(
        Guid subscriptionPlanId,
        string name,
        string? description,
        decimal price,
        string currency,
        string billingInterval,
        int billingIntervalCount,
        bool isActive,
        bool isFeatured,
        int sortOrder,
        string? providerProductId,
        string? providerPriceId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<AdminPaymentRecordDto>> GetAdminPaymentRecordsAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<RefundRecordDto> CreateAdminRefundAsync(
        Guid paymentAttemptId,
        decimal? amount,
        string? reason,
        string adminId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminSubscriptionAllocationRunDto>> GetAdminSubscriptionAllocationRunsAsync(
        int take = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentMethodRefDto>> GetMyPaymentMethodsAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    Task RemoveMyPaymentMethodAsync(
        Guid paymentMethodId,
        string studentId,
        CancellationToken cancellationToken = default);

    Task<PaymentAttemptDto?> GetPaymentAttemptAsync(
        Guid paymentAttemptId,
        string studentId,
        CancellationToken cancellationToken = default);

    Task<PaymentAttemptDto?> MarkPaymentAttemptCanceledAsync(
        Guid paymentAttemptId,
        string studentId,
        CancellationToken cancellationToken = default);

    Task HandleStripeWebhookAsync(
        string payload,
        string? signatureHeader,
        CancellationToken cancellationToken = default);
}

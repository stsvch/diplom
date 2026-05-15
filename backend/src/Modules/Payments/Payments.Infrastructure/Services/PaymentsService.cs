// PaymentsService.cs
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Payments.Application.DTOs;
using Payments.Application.Interfaces;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Infrastructure.Configuration;
using System.Linq.Expressions;

namespace Payments.Infrastructure.Services;

// Основной тип файла описывает часть модуля и его публичный контракт.
public class PaymentsService : IPaymentsService, ITeacherPayoutReadService
{
    private readonly IPaymentsDbContext _context;
    private readonly IPaymentProviderGateway _gateway;
    private readonly ICoursePaymentReadService _coursePaymentReadService;
    private readonly IEnrollmentReadService _enrollmentReadService;
    private readonly ICourseAccessProvisioningService _courseAccessProvisioningService;
    private readonly ISubscriptionEntitlementProvider _entitlementProvider;
    private readonly PaymentsOptions _paymentsOptions;
    private readonly IConfiguration _configuration;

    public PaymentsService(
        IPaymentsDbContext context,
        IPaymentProviderGateway gateway,
        ICoursePaymentReadService coursePaymentReadService,
        IEnrollmentReadService enrollmentReadService,
        ICourseAccessProvisioningService courseAccessProvisioningService,
        ISubscriptionEntitlementProvider entitlementProvider,
        IOptions<PaymentsOptions> paymentsOptions,
        IConfiguration configuration)
    {
        _context = context;
        _gateway = gateway;
        _coursePaymentReadService = coursePaymentReadService;
        _enrollmentReadService = enrollmentReadService;
        _courseAccessProvisioningService = courseAccessProvisioningService;
        _entitlementProvider = entitlementProvider;
        _paymentsOptions = paymentsOptions.Value;
        _configuration = configuration;
    }

    public async Task<UserEntitlementsDto> GetMyEntitlementsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var ent = await _entitlementProvider.GetForUserAsync(userId, cancellationToken);
        return new UserEntitlementsDto(
            ent.HasActiveSubscription,
            ent.PlanName,
            ent.IndividualSlotsPerMonth,
            ent.IndividualSlotsUsed,
            ent.IndividualSlotsRemaining,
            ent.GroupSlotsPerMonth,
            ent.GroupSlotsUsed,
            ent.GroupSlotsRemaining,
            ent.CurrentPeriodStart,
            ent.CurrentPeriodEnd);
    }

    public async Task<TeacherPayoutAccountDto> GetTeacherPayoutAccountAsync(
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        var account = await _context.TeacherPayoutAccounts
            .FirstOrDefaultAsync(x => x.TeacherId == teacherId, cancellationToken);

        if (account != null && _gateway.IsConfigured && !string.IsNullOrWhiteSpace(account.ProviderAccountId))
        {
            var snapshot = await _gateway.GetTeacherAccountAsync(account.ProviderAccountId, cancellationToken);
            ApplyProviderSnapshot(account, snapshot);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return MapTeacherPayoutAccount(account, _gateway.IsConfigured);
    }

    public async Task<string> CreateTeacherOnboardingLinkAsync(
        string teacherId,
        string teacherEmail,
        string teacherName,
        CancellationToken cancellationToken = default)
    {
        EnsureProviderConfigured();

        var account = await _context.TeacherPayoutAccounts
            .FirstOrDefaultAsync(x => x.TeacherId == teacherId, cancellationToken);

        if (account == null || string.IsNullOrWhiteSpace(account.ProviderAccountId))
        {
            var created = await _gateway.CreateTeacherAccountAsync(
                teacherId,
                teacherEmail,
                teacherName,
                cancellationToken);

            if (account == null)
            {
                account = new TeacherPayoutAccount
                {
                    TeacherId = teacherId,
                    Provider = _paymentsOptions.Provider,
                    ProviderAccountId = created.ProviderAccountId,
                    OnboardingStartedAt = DateTime.UtcNow,
                };
                _context.TeacherPayoutAccounts.Add(account);
            }

            ApplyProviderSnapshot(account, created);
            account.OnboardingStartedAt ??= DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
        var refreshUrl = $"{frontendUrl}/teacher/payments";
        var returnUrl = $"{frontendUrl}/teacher/payments";

        return await _gateway.CreateTeacherOnboardingLinkAsync(
            account.ProviderAccountId,
            refreshUrl,
            returnUrl,
            cancellationToken);
    }

    public async Task<string> CreateTeacherDashboardLinkAsync(
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        EnsureProviderConfigured();

        var account = await _context.TeacherPayoutAccounts
            .FirstOrDefaultAsync(x => x.TeacherId == teacherId, cancellationToken);

        if (account == null || string.IsNullOrWhiteSpace(account.ProviderAccountId))
            throw new InvalidOperationException("Сначала подключите payout account преподавателя.");

        return await _gateway.CreateTeacherDashboardLinkAsync(
            account.ProviderAccountId,
            cancellationToken);
    }

    public async Task<TeacherSettlementSummaryDto> GetTeacherSettlementSummaryAsync(
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        await RefreshTeacherSettlementsAsync(teacherId, cancellationToken);

        var settlements = await _context.TeacherSettlements
            .Where(x => x.TeacherId == teacherId)
            .ToListAsync(cancellationToken);

        return new TeacherSettlementSummaryDto(
            settlements.Sum(x => x.GrossAmount),
            settlements.Sum(GetRemainingNetAmount),
            settlements
                .Where(x => x.Status == TeacherSettlementStatus.PendingHold)
                .Sum(GetRemainingNetAmount),
            settlements
                .Where(x => x.Status == TeacherSettlementStatus.ReadyForPayout)
                .Sum(GetRemainingNetAmount),
            settlements
                .Where(x => x.Status == TeacherSettlementStatus.InPayout)
                .Sum(GetRemainingNetAmount),
            settlements
                .Where(x => x.Status == TeacherSettlementStatus.PaidOut)
                .Sum(GetRemainingNetAmount),
            settlements.Count,
            _paymentsOptions.Currency);
    }

    public async Task<IReadOnlyList<TeacherSettlementDto>> GetTeacherSettlementsAsync(
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        await RefreshTeacherSettlementsAsync(teacherId, cancellationToken);

        return await _context.TeacherSettlements
            .Where(x => x.TeacherId == teacherId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new TeacherSettlementDto(
                x.Id,
                x.CourseId,
                x.CourseTitle,
                x.StudentName,
                x.GrossAmount,
                x.ProviderFeeAmount,
                x.PlatformCommissionAmount,
                x.NetAmount,
                x.Currency,
                x.Status.ToString(),
                x.AvailableAt,
                x.PaidOutAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionPlanDto>> GetActiveSubscriptionPlansAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionPlans
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.IsFeatured)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Price)
            .Select(MapSubscriptionPlanProjection())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionPlanDto>> GetAdminSubscriptionPlansAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionPlans
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.IsFeatured)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Price)
            .Select(MapSubscriptionPlanProjection())
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionPlanDto> CreateSubscriptionPlanAsync(
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
        int individualSlotsPerMonth,
        int groupSlotsPerMonth,
        CancellationToken cancellationToken = default)
    {
        var plan = new SubscriptionPlan();
        ApplySubscriptionPlanInput(
            plan,
            name,
            description,
            price,
            currency,
            billingInterval,
            billingIntervalCount,
            isActive,
            isFeatured,
            sortOrder,
            providerProductId,
            providerPriceId,
            individualSlotsPerMonth,
            groupSlotsPerMonth);

        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);
        return MapSubscriptionPlan(plan);
    }

    public async Task<SubscriptionPlanDto> UpdateSubscriptionPlanAsync(
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
        int individualSlotsPerMonth,
        int groupSlotsPerMonth,
        CancellationToken cancellationToken = default)
    {
        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(x => x.Id == subscriptionPlanId, cancellationToken);
        if (plan == null)
            throw new InvalidOperationException("Тариф подписки не найден.");

        ApplySubscriptionPlanInput(
            plan,
            name,
            description,
            price,
            currency,
            billingInterval,
            billingIntervalCount,
            isActive,
            isFeatured,
            sortOrder,
            providerProductId,
            providerPriceId,
            individualSlotsPerMonth,
            groupSlotsPerMonth);

        await _context.SaveChangesAsync(cancellationToken);
        return MapSubscriptionPlan(plan);
    }

    public async Task<PagedResult<AdminPaymentRecordDto>> GetAdminPaymentRecordsAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var normalizedSearch = search?.Trim().ToLowerInvariant();

        var query =
            from attempt in _context.PaymentAttempts
            join purchase in _context.CoursePurchases on attempt.Id equals purchase.PaymentAttemptId into purchaseGroup
            from purchase in purchaseGroup.DefaultIfEmpty()
            select new { attempt, purchase };

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(x =>
                x.attempt.CourseTitle.ToLower().Contains(normalizedSearch)
                || x.attempt.StudentName.ToLower().Contains(normalizedSearch)
                || x.attempt.StudentId.ToLower().Contains(normalizedSearch)
                || x.attempt.TeacherId.ToLower().Contains(normalizedSearch)
                || (x.attempt.ProviderChargeId != null && x.attempt.ProviderChargeId.ToLower().Contains(normalizedSearch)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var pageItems = await query
            .OrderByDescending(x => x.attempt.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<AdminPaymentRecordDto>(pageItems.Count);
        foreach (var item in pageItems)
        {
            var courseInfo = await _coursePaymentReadService.GetCoursePaymentInfoAsync(
                item.attempt.CourseId,
                cancellationToken);
            var settlement = await _context.TeacherSettlements
                .FirstOrDefaultAsync(x => x.PaymentAttemptId == item.attempt.Id, cancellationToken);

            items.Add(new AdminPaymentRecordDto(
                item.attempt.Id,
                item.attempt.CourseId,
                item.attempt.CourseTitle,
                item.attempt.StudentId,
                item.attempt.StudentName,
                item.attempt.TeacherId,
                courseInfo?.TeacherName ?? item.attempt.TeacherId,
                item.attempt.Amount,
                settlement?.ProviderFeeAmount ?? 0m,
                item.attempt.Currency,
                item.attempt.Status.ToString(),
                item.attempt.ProviderChargeId,
                item.purchase?.Status.ToString(),
                item.attempt.CreatedAt,
                item.attempt.CompletedAt));
        }

        return new PagedResult<AdminPaymentRecordDto>(items, totalCount, page, pageSize);
    }

    public async Task<IReadOnlyList<PayoutRecordDto>> GetTeacherPayoutRecordsAsync(
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PayoutRecords
            .Where(x => x.TeacherId == teacherId)
            .OrderByDescending(x => x.RequestedAt)
            .Select(x => new PayoutRecordDto(
                x.Id,
                x.Amount,
                x.Currency,
                x.SettlementsCount,
                x.Status.ToString(),
                x.ProviderTransferId,
                x.RequestedAt,
                x.SubmittedAt,
                x.PaidAt,
                x.FailedAt,
                x.FailureMessage))
            .ToListAsync(cancellationToken);
    }

    public async Task<PayoutRecordDto> RequestTeacherPayoutAsync(
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        EnsureProviderConfigured();
        await RefreshTeacherSettlementsAsync(teacherId, cancellationToken);

        var payoutAccount = await _context.TeacherPayoutAccounts
            .FirstOrDefaultAsync(x => x.TeacherId == teacherId, cancellationToken);

        var payoutAccountDto = await GetTeacherPayoutAccountAsync(teacherId, cancellationToken);
        if (!payoutAccountDto.CanPublishPaidCourses || payoutAccount == null)
            throw new InvalidOperationException("Выплаты недоступны, пока payout account не готов.");
        if (string.IsNullOrWhiteSpace(payoutAccount.ProviderAccountId))
            throw new InvalidOperationException("У payout account преподавателя нет provider account id.");

        var settlements = await _context.TeacherSettlements
            .Where(x => x.TeacherId == teacherId
                     && x.Status == TeacherSettlementStatus.ReadyForPayout
                     && x.PayoutRecordId == null)
            .OrderBy(x => x.AvailableAt)
            .ToListAsync(cancellationToken);

        var payableSettlements = settlements
            .Where(x => GetRemainingNetAmount(x) > 0)
            .ToList();

        if (payableSettlements.Count == 0)
            throw new InvalidOperationException("Нет начислений, готовых к выплате.");

        var payoutCurrency = payableSettlements[0].Currency;

        var payoutRecord = new PayoutRecord
        {
            TeacherId = teacherId,
            Provider = _paymentsOptions.Provider,
            ProviderAccountId = payoutAccount.ProviderAccountId,
            Amount = payableSettlements.Sum(GetRemainingNetAmount),
            Currency = payoutCurrency,
            SettlementsCount = payableSettlements.Count,
            Status = PayoutRecordStatus.Queued,
            RequestedAt = DateTime.UtcNow,
        };

        try
        {
            var providerTransfer = await _gateway.CreateTransferAsync(
                new ProviderTransferRequest(
                    payoutRecord.Id,
                    teacherId,
                    payoutAccount.ProviderAccountId,
                    payoutRecord.Amount,
                    payoutRecord.Currency,
                    payoutRecord.SettlementsCount),
                cancellationToken);

            payoutRecord.ProviderTransferId = providerTransfer.ProviderTransferId;
            payoutRecord.Status = PayoutRecordStatus.SubmittedToProvider;
            payoutRecord.SubmittedAt = DateTime.UtcNow;
            payoutRecord.FailureMessage = null;
        }
        catch (InvalidOperationException ex)
        {
            payoutRecord.Status = PayoutRecordStatus.Failed;
            payoutRecord.FailedAt = DateTime.UtcNow;
            payoutRecord.FailureMessage = ex.Message;
            _context.PayoutRecords.Add(payoutRecord);
            await _context.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException($"Провайдер не принял выплату: {ex.Message}");
        }

        _context.PayoutRecords.Add(payoutRecord);

        foreach (var settlement in payableSettlements)
        {
            settlement.PayoutRecordId = payoutRecord.Id;
            settlement.Status = TeacherSettlementStatus.InPayout;
        }

        if (IsLocalProvider())
        {
            payoutRecord.Status = PayoutRecordStatus.Paid;
            payoutRecord.PaidAt = DateTime.UtcNow;
            payoutRecord.FailureMessage = null;

            foreach (var settlement in payableSettlements)
            {
                settlement.Status = TeacherSettlementStatus.PaidOut;
                settlement.PaidOutAt ??= payoutRecord.PaidAt;
            }

        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapPayoutRecord(payoutRecord);
    }

    public async Task<CourseCheckoutSessionDto> CreateCourseCheckoutAsync(
        Guid courseId,
        string studentId,
        string studentEmail,
        string studentName,
        bool savePaymentMethod,
        CancellationToken cancellationToken = default)
    {
        EnsureProviderConfigured();

        var course = await _coursePaymentReadService.GetCoursePaymentInfoAsync(courseId, cancellationToken);
        if (course == null)
            throw new InvalidOperationException("Курс не найден.");
        if (!course.IsPublished || course.IsArchived)
            throw new InvalidOperationException("Курс недоступен для покупки.");
        if (course.IsFree || !course.Price.HasValue || course.Price.Value <= 0)
            throw new InvalidOperationException("Этот курс не требует оплаты.");

        var teacherReadyForPaidCourses = await IsTeacherReadyForPaidCoursesAsync(
            course.TeacherId,
            cancellationToken);
        if (!teacherReadyForPaidCourses)
            throw new InvalidOperationException("Преподаватель временно не может принимать оплату за этот курс.");

        var activeCourseIds = await _enrollmentReadService.GetActiveCourseIdsForStudentAsync(studentId, cancellationToken);
        if (activeCourseIds.Contains(courseId))
            throw new InvalidOperationException("У вас уже есть доступ к этому курсу.");

        var existingPurchase = await _context.CoursePurchases
            .AnyAsync(x => x.StudentId == studentId
                        && x.CourseId == courseId
                        && x.Status == CoursePurchaseStatus.Active, cancellationToken);
        if (existingPurchase)
            throw new InvalidOperationException("Курс уже куплен.");

        var paymentProfile = await _context.UserPaymentProfiles
            .FirstOrDefaultAsync(x => x.UserId == studentId, cancellationToken);

        if (paymentProfile == null)
        {
            var providerCustomerId = await _gateway.CreateCustomerAsync(
                studentId,
                studentEmail,
                studentName,
                cancellationToken);

            paymentProfile = new UserPaymentProfile
            {
                UserId = studentId,
                Provider = _paymentsOptions.Provider,
                ProviderCustomerId = providerCustomerId,
            };

            _context.UserPaymentProfiles.Add(paymentProfile);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var attempt = new PaymentAttempt
        {
            CourseId = course.CourseId,
            CourseTitle = course.Title,
            TeacherId = course.TeacherId,
            StudentId = studentId,
            StudentName = studentName,
            Amount = course.Price.Value,
            Currency = _paymentsOptions.Currency,
            Provider = _paymentsOptions.Provider,
            Status = PaymentAttemptStatus.Initiated,
            SavePaymentMethodRequested = savePaymentMethod,
            ProviderCustomerId = paymentProfile.ProviderCustomerId,
        };

        _context.PaymentAttempts.Add(attempt);
        await _context.SaveChangesAsync(cancellationToken);

        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
        var successUrl = $"{frontendUrl}/student/payments?attempt={attempt.Id}&state=success";
        var cancelUrl = $"{frontendUrl}/student/payments?attempt={attempt.Id}&state=cancel";

        var checkout = await _gateway.CreateCourseCheckoutSessionAsync(
            new ProviderCheckoutSessionRequest(
                paymentProfile.ProviderCustomerId,
                _paymentsOptions.Currency,
                course.Price.Value,
                course.Title,
                attempt.Id,
                course.CourseId,
                course.TeacherId,
                studentId,
                successUrl,
                cancelUrl,
                savePaymentMethod),
            cancellationToken);

        attempt.ProviderSessionId = checkout.SessionId;
        attempt.Status = PaymentAttemptStatus.PendingProvider;
        await _context.SaveChangesAsync(cancellationToken);

        if (IsLocalProvider())
        {
            await FinalizeCoursePaymentAsync(
                attempt,
                CreateLocalCoursePaymentWebhook(attempt, checkout.SessionId),
                cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new CourseCheckoutSessionDto(attempt.Id, checkout.CheckoutUrl);
    }

    public async Task<SubscriptionCheckoutSessionDto> CreateSubscriptionCheckoutAsync(
        Guid subscriptionPlanId,
        string studentId,
        string studentEmail,
        string studentName,
        CancellationToken cancellationToken = default)
    {
        EnsureProviderConfigured();

        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(x => x.Id == subscriptionPlanId && x.IsActive, cancellationToken);
        if (plan == null)
            throw new InvalidOperationException("Тариф подписки не найден или недоступен.");

        var hasBlockingSubscription = await _context.UserSubscriptions
            .AnyAsync(
                x => x.UserId == studentId
                  && x.Status != UserSubscriptionStatus.Canceled,
                cancellationToken);
        if (hasBlockingSubscription)
            throw new InvalidOperationException("У вас уже есть активная или незавершённая подписка.");

        var paymentProfile = await GetOrCreatePaymentProfileAsync(
            studentId,
            studentEmail,
            studentName,
            cancellationToken);

        var attempt = new SubscriptionPaymentAttempt
        {
            SubscriptionPlanId = plan.Id,
            UserId = studentId,
            PlanName = plan.Name,
            Amount = plan.Price,
            Currency = plan.Currency,
            BillingInterval = plan.BillingInterval,
            BillingIntervalCount = plan.BillingIntervalCount,
            Provider = _paymentsOptions.Provider,
            Status = SubscriptionPaymentAttemptStatus.Initiated,
            ProviderCustomerId = paymentProfile.ProviderCustomerId,
        };

        _context.SubscriptionPaymentAttempts.Add(attempt);
        await _context.SaveChangesAsync(cancellationToken);

        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
        var successUrl = $"{frontendUrl}/student/payments?subscriptionAttempt={attempt.Id}&subscriptionState=success";
        var cancelUrl = $"{frontendUrl}/student/payments?subscriptionAttempt={attempt.Id}&subscriptionState=cancel";

        var checkout = await _gateway.CreateSubscriptionCheckoutSessionAsync(
            new ProviderSubscriptionCheckoutSessionRequest(
                paymentProfile.ProviderCustomerId,
                plan.Currency,
                plan.Price,
                plan.Name,
                attempt.Id,
                plan.Id,
                studentId,
                successUrl,
                cancelUrl,
                plan.BillingInterval.ToString(),
                plan.BillingIntervalCount),
            cancellationToken);

        attempt.ProviderSessionId = checkout.SessionId;
        attempt.Status = SubscriptionPaymentAttemptStatus.PendingProvider;
        await _context.SaveChangesAsync(cancellationToken);

        if (IsLocalProvider())
        {
            attempt.Status = SubscriptionPaymentAttemptStatus.Succeeded;
            attempt.ProviderSubscriptionId = $"sub_local_{attempt.Id:N}";
            attempt.CompletedAt ??= DateTime.UtcNow;
            attempt.FailureMessage = null;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new SubscriptionCheckoutSessionDto(attempt.Id, checkout.CheckoutUrl);
    }

    public async Task<IReadOnlyList<PaymentAttemptDto>> GetMyPaymentHistoryAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PaymentAttempts
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PaymentAttemptDto(
                x.Id,
                x.CourseId,
                x.CourseTitle,
                x.Amount,
                x.Currency,
                x.Status.ToString(),
                x.ProviderChargeId,
                x.FailureMessage,
                x.CreatedAt,
                x.CompletedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserSubscriptionDto>> GetMySubscriptionsAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserSubscriptions
            .Where(x => x.UserId == studentId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new UserSubscriptionDto(
                x.Id,
                x.SubscriptionPlanId,
                x.PlanName,
                x.Price,
                x.Currency,
                x.Status.ToString(),
                x.CurrentPeriodStart,
                x.CurrentPeriodEnd,
                x.CancelAtPeriodEnd,
                x.CanceledAt,
                x.StartedAt,
                x.EndedAt,
                _context.SubscriptionPlans
                    .Where(p => p.Id == x.SubscriptionPlanId)
                    .Select(p => p.IndividualSlotsPerMonth)
                    .FirstOrDefault(),
                _context.SubscriptionPlans
                    .Where(p => p.Id == x.SubscriptionPlanId)
                    .Select(p => p.GroupSlotsPerMonth)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionPaymentAttemptDto>> GetMySubscriptionHistoryAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionPaymentAttempts
            .Where(x => x.UserId == studentId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SubscriptionPaymentAttemptDto(
                x.Id,
                x.SubscriptionPlanId,
                x.PlanName,
                x.Amount,
                x.Currency,
                x.BillingInterval.ToString(),
                x.BillingIntervalCount,
                x.Status.ToString(),
                x.FailureMessage,
                x.CreatedAt,
                x.CompletedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionInvoiceDto>> GetMySubscriptionInvoicesAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionInvoices
            .Where(x => x.UserId == studentId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SubscriptionInvoiceDto(
                x.Id,
                x.SubscriptionPlanId,
                x.PlanName,
                x.AmountDue,
                x.AmountPaid,
                x.Currency,
                x.Status.ToString(),
                x.BillingReason,
                x.PeriodStart,
                x.PeriodEnd,
                x.DueDate,
                x.PaidAt,
                x.FailureMessage,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionPaymentAttemptDto?> GetSubscriptionPaymentAttemptAsync(
        Guid subscriptionPaymentAttemptId,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionPaymentAttempts
            .Where(x => x.Id == subscriptionPaymentAttemptId && x.UserId == studentId)
            .Select(x => new SubscriptionPaymentAttemptDto(
                x.Id,
                x.SubscriptionPlanId,
                x.PlanName,
                x.Amount,
                x.Currency,
                x.BillingInterval.ToString(),
                x.BillingIntervalCount,
                x.Status.ToString(),
                x.FailureMessage,
                x.CreatedAt,
                x.CompletedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CoursePurchaseDto>> GetMyPurchasesAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CoursePurchases
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.PurchasedAt)
            .Select(x => new CoursePurchaseDto(
                x.Id,
                x.CourseId,
                x.CourseTitle,
                x.Amount,
                x.Currency,
                x.Status.ToString(),
                x.PurchasedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentMethodRefDto>> GetMyPaymentMethodsAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods
            .Where(x => x.UserId == studentId && x.RemovedAt == null)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new PaymentMethodRefDto(
                x.Id,
                x.Brand,
                x.Last4,
                x.ExpMonth,
                x.ExpYear,
                x.IsDefault))
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveMyPaymentMethodAsync(
        Guid paymentMethodId,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        EnsureProviderConfigured();

        var paymentMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(
                x => x.Id == paymentMethodId
                  && x.UserId == studentId
                  && x.RemovedAt == null,
                cancellationToken);

        if (paymentMethod == null)
            throw new InvalidOperationException("Сохранённый способ оплаты не найден.");

        await _gateway.DetachPaymentMethodAsync(
            paymentMethod.ProviderPaymentMethodId,
            cancellationToken);

        paymentMethod.IsDefault = false;
        paymentMethod.RemovedAt = DateTime.UtcNow;

        await NormalizeActivePaymentMethodDefaultsAsync(studentId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaymentAttemptDto?> GetPaymentAttemptAsync(
        Guid paymentAttemptId,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PaymentAttempts
            .Where(x => x.Id == paymentAttemptId && x.StudentId == studentId)
            .Select(MapPaymentAttemptProjection())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaymentAttemptDto?> MarkPaymentAttemptCanceledAsync(
        Guid paymentAttemptId,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        var attempt = await _context.PaymentAttempts
            .FirstOrDefaultAsync(x => x.Id == paymentAttemptId && x.StudentId == studentId, cancellationToken);
        if (attempt == null)
            return null;

        if (attempt.Status is PaymentAttemptStatus.Initiated or PaymentAttemptStatus.PendingProvider
            && !attempt.CompletedAt.HasValue)
        {
            attempt.Status = PaymentAttemptStatus.Canceled;
            attempt.FailureCode = null;
            attempt.FailureMessage = "Checkout был отменён до подтверждения оплаты.";
            await _context.SaveChangesAsync(cancellationToken);
        }

        return MapPaymentAttempt(attempt);
    }

    public async Task HandleStripeWebhookAsync(
        string payload,
        string? signatureHeader,
        CancellationToken cancellationToken = default)
    {
        EnsureProviderConfigured();

        var webhook = _gateway.ParseWebhook(payload, signatureHeader);
        var alreadyProcessed = await _context.ProcessedWebhookEvents
            .AnyAsync(x => x.Provider == _paymentsOptions.Provider
                        && x.ProviderEventId == webhook.EventId, cancellationToken);
        if (alreadyProcessed)
            return;

        switch (webhook.EventType)
        {
            case "account.updated":
                await HandleAccountUpdatedAsync(webhook, cancellationToken);
                break;
            case "checkout.session.completed":
                await HandleCheckoutCompletedAsync(webhook, cancellationToken);
                break;
            case "checkout.session.async_payment_succeeded":
                await HandleCoursePaymentSucceededAsync(webhook, cancellationToken);
                break;
            case "checkout.session.async_payment_failed":
                await HandleCoursePaymentFailedAsync(webhook, cancellationToken);
                break;
            case "checkout.session.expired":
                await HandleCheckoutExpiredAsync(webhook, cancellationToken);
                break;
            case "payment_intent.succeeded":
                await HandleCoursePaymentSucceededAsync(webhook, cancellationToken);
                break;
            case "payment_intent.payment_failed":
                await HandlePaymentIntentFailedAsync(webhook, cancellationToken);
                break;
            case "transfer.created":
                await HandleTransferCreatedAsync(webhook, cancellationToken);
                break;
            case "transfer.reversed":
                await HandleTransferReversedAsync(webhook, cancellationToken);
                break;
            case "customer.subscription.created":
            case "customer.subscription.updated":
            case "customer.subscription.deleted":
                await HandleSubscriptionWebhookAsync(webhook, cancellationToken);
                break;
            case var _ when webhook.EventType.StartsWith("invoice.", StringComparison.OrdinalIgnoreCase):
                await HandleSubscriptionInvoiceWebhookAsync(webhook, cancellationToken);
                break;
        }

        _context.ProcessedWebhookEvents.Add(new ProcessedWebhookEvent
        {
            Provider = _paymentsOptions.Provider,
            ProviderEventId = webhook.EventId,
            EventType = webhook.EventType,
            ProcessedAt = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsTeacherReadyForPaidCoursesAsync(
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        if (!_gateway.IsConfigured)
            return false;

        var dto = await GetTeacherPayoutAccountAsync(teacherId, cancellationToken);
        return dto.CanPublishPaidCourses;
    }

    private async Task HandleAccountUpdatedAsync(StripeWebhookEvent webhook, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(webhook.ProviderAccountId))
            return;

        var account = await _context.TeacherPayoutAccounts
            .FirstOrDefaultAsync(x => x.ProviderAccountId == webhook.ProviderAccountId, cancellationToken);
        if (account == null)
            return;

        ApplyProviderSnapshot(account, new ProviderTeacherAccountResult(
            webhook.ProviderAccountId,
            webhook.ChargesEnabled ?? false,
            webhook.PayoutsEnabled ?? false,
            webhook.DetailsSubmitted ?? false,
            webhook.RequirementsSummary));
    }

    private async Task HandleCheckoutCompletedAsync(StripeWebhookEvent webhook, CancellationToken cancellationToken)
    {
        if (webhook.Metadata.ContainsKey("subscriptionPaymentAttemptId"))
        {
            await HandleSubscriptionCheckoutCompletedAsync(webhook, cancellationToken);
            return;
        }

        if (!webhook.Metadata.TryGetValue("paymentAttemptId", out var paymentAttemptIdRaw)
            || !Guid.TryParse(paymentAttemptIdRaw, out var paymentAttemptId))
        {
            return;
        }

        var attempt = await _context.PaymentAttempts
            .FirstOrDefaultAsync(x => x.Id == paymentAttemptId, cancellationToken);
        if (attempt == null)
            return;

        ApplyCourseAttemptWebhookData(attempt, webhook);

        if (string.Equals(webhook.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
        {
            await FinalizeCoursePaymentAsync(attempt, webhook, cancellationToken);
            return;
        }

        attempt.Status = PaymentAttemptStatus.PendingProvider;
        attempt.FailureCode = null;
        attempt.FailureMessage = null;
    }

    private async Task HandleCoursePaymentSucceededAsync(
        StripeWebhookEvent webhook,
        CancellationToken cancellationToken)
    {
        var attempt = await FindCoursePaymentAttemptAsync(webhook, cancellationToken);
        if (attempt == null)
            return;

        ApplyCourseAttemptWebhookData(attempt, webhook);
        await FinalizeCoursePaymentAsync(attempt, webhook, cancellationToken);
    }

    private async Task HandleCoursePaymentFailedAsync(
        StripeWebhookEvent webhook,
        CancellationToken cancellationToken)
    {
        var attempt = await FindCoursePaymentAttemptAsync(webhook, cancellationToken);
        if (attempt == null || attempt.Status == PaymentAttemptStatus.Succeeded)
            return;

        ApplyCourseAttemptWebhookData(attempt, webhook);
        attempt.Status = PaymentAttemptStatus.Failed;
        attempt.FailureCode = null;
        attempt.FailureMessage = webhook.FailureMessage ?? "Провайдер не подтвердил оплату.";
    }

    private async Task HandleSubscriptionCheckoutCompletedAsync(
        StripeWebhookEvent webhook,
        CancellationToken cancellationToken)
    {
        if (!webhook.Metadata.TryGetValue("subscriptionPaymentAttemptId", out var attemptIdRaw)
            || !Guid.TryParse(attemptIdRaw, out var attemptId))
        {
            return;
        }

        var attempt = await _context.SubscriptionPaymentAttempts
            .FirstOrDefaultAsync(x => x.Id == attemptId, cancellationToken);
        if (attempt == null)
            return;

        attempt.ProviderSessionId ??= webhook.SessionId;
        attempt.ProviderCustomerId ??= webhook.CustomerId;
        attempt.ProviderSubscriptionId ??= webhook.ProviderSubscriptionId;
        attempt.FailureMessage = null;

        if (string.Equals(webhook.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
        {
            attempt.Status = SubscriptionPaymentAttemptStatus.Succeeded;
            attempt.CompletedAt ??= DateTime.UtcNow;
        }
        else
        {
            attempt.Status = SubscriptionPaymentAttemptStatus.PendingProvider;
        }
    }

    private async Task HandleSubscriptionCheckoutExpiredAsync(
        StripeWebhookEvent webhook,
        CancellationToken cancellationToken)
    {
        if (!webhook.Metadata.TryGetValue("subscriptionPaymentAttemptId", out var attemptIdRaw)
            || !Guid.TryParse(attemptIdRaw, out var attemptId))
        {
            return;
        }

        var attempt = await _context.SubscriptionPaymentAttempts
            .FirstOrDefaultAsync(x => x.Id == attemptId, cancellationToken);
        if (attempt == null || attempt.Status == SubscriptionPaymentAttemptStatus.Succeeded)
            return;

        attempt.Status = SubscriptionPaymentAttemptStatus.Expired;
        attempt.FailureMessage = "Сессия оформления подписки истекла.";
    }

    private async Task<PaymentAttempt?> FindCoursePaymentAttemptAsync(
        StripeWebhookEvent webhook,
        CancellationToken cancellationToken)
    {
        if (webhook.Metadata.TryGetValue("paymentAttemptId", out var paymentAttemptIdRaw)
            && Guid.TryParse(paymentAttemptIdRaw, out var paymentAttemptId))
        {
            return await _context.PaymentAttempts
                .FirstOrDefaultAsync(x => x.Id == paymentAttemptId, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(webhook.PaymentIntentId))
        {
            var attemptByPaymentIntent = await _context.PaymentAttempts
                .FirstOrDefaultAsync(x => x.ProviderPaymentIntentId == webhook.PaymentIntentId, cancellationToken);
            if (attemptByPaymentIntent != null)
                return attemptByPaymentIntent;
        }

        if (!string.IsNullOrWhiteSpace(webhook.SessionId))
        {
            return await _context.PaymentAttempts
                .FirstOrDefaultAsync(x => x.ProviderSessionId == webhook.SessionId, cancellationToken);
        }

        return null;
    }

    private static void ApplyCourseAttemptWebhookData(
        PaymentAttempt attempt,
        StripeWebhookEvent webhook)
    {
        attempt.ProviderSessionId ??= webhook.SessionId;
        attempt.ProviderPaymentIntentId ??= webhook.PaymentIntentId;
        attempt.ProviderCustomerId ??= webhook.CustomerId;
    }

    private StripeWebhookEvent CreateLocalCoursePaymentWebhook(PaymentAttempt attempt, string providerSessionId)
    {
        return new StripeWebhookEvent(
            EventId: $"evt_local_checkout_{attempt.Id:N}",
            EventType: "checkout.session.completed",
            ProviderAccountId: null,
            ProviderTransferId: null,
            ProviderInvoiceId: null,
            SessionId: providerSessionId,
            ProviderSubscriptionId: null,
            PaymentIntentId: $"pi_local_{attempt.Id:N}",
            CustomerId: attempt.ProviderCustomerId,
            SubscriptionStatus: null,
            InvoiceStatus: null,
            InvoiceBillingReason: null,
            PaymentStatus: "paid",
            FailureMessage: null,
            CurrentPeriodStart: null,
            CurrentPeriodEnd: null,
            CancelAtPeriodEnd: null,
            SubscriptionCanceledAt: null,
            InvoiceDueDate: null,
            InvoicePaidAt: null,
            AmountMinor: ToMinorUnits(attempt.Amount),
            AmountDueMinor: null,
            AmountPaidMinor: ToMinorUnits(attempt.Amount),
            Currency: attempt.Currency,
            Metadata: new Dictionary<string, string>
            {
                ["paymentAttemptId"] = attempt.Id.ToString(),
                ["courseId"] = attempt.CourseId.ToString(),
                ["teacherId"] = attempt.TeacherId,
                ["studentId"] = attempt.StudentId,
            },
            ChargesEnabled: null,
            PayoutsEnabled: null,
            DetailsSubmitted: null,
            RequirementsSummary: null);
    }

    private async Task FinalizeCoursePaymentAsync(
        PaymentAttempt attempt,
        StripeWebhookEvent webhook,
        CancellationToken cancellationToken)
    {
        ApplyCourseAttemptWebhookData(attempt, webhook);
        attempt.Status = PaymentAttemptStatus.Succeeded;
        attempt.CompletedAt ??= DateTime.UtcNow;
        attempt.FailureCode = null;
        attempt.FailureMessage = null;

        ProviderChargeSnapshot? chargeSnapshot = null;
        if (!string.IsNullOrWhiteSpace(attempt.ProviderPaymentIntentId))
        {
            chargeSnapshot = await _gateway.GetPaymentChargeSnapshotAsync(
                attempt.ProviderPaymentIntentId,
                cancellationToken);

            if (chargeSnapshot != null)
                attempt.ProviderChargeId = chargeSnapshot.ProviderChargeId;
        }

        var purchase = await _context.CoursePurchases
            .FirstOrDefaultAsync(x => x.PaymentAttemptId == attempt.Id, cancellationToken);

        if (purchase == null)
        {
            purchase = new CoursePurchase
            {
                CourseId = attempt.CourseId,
                CourseTitle = attempt.CourseTitle,
                TeacherId = attempt.TeacherId,
                StudentId = attempt.StudentId,
                PaymentAttemptId = attempt.Id,
                Amount = attempt.Amount,
                Currency = attempt.Currency,
                Status = CoursePurchaseStatus.Active,
                PurchasedAt = attempt.CompletedAt ?? DateTime.UtcNow,
            };

            _context.CoursePurchases.Add(purchase);
        }
        else
        {
            purchase.CourseTitle = attempt.CourseTitle;
            purchase.TeacherId = attempt.TeacherId;
            purchase.Amount = attempt.Amount;
            purchase.Currency = attempt.Currency;
            purchase.Status = CoursePurchaseStatus.Active;
            purchase.PurchasedAt = attempt.CompletedAt ?? DateTime.UtcNow;
        }

        await EnsureTeacherSettlementExistsAsync(
            attempt,
            purchase,
            chargeSnapshot?.ProviderFeeAmount ?? 0m,
            cancellationToken);

        if (attempt.SavePaymentMethodRequested && !string.IsNullOrWhiteSpace(attempt.ProviderPaymentIntentId))
        {
            var paymentMethod = await _gateway.GetPaymentMethodSnapshotAsync(
                attempt.ProviderPaymentIntentId,
                cancellationToken);

            if (paymentMethod != null)
            {
                attempt.ProviderPaymentMethodId = paymentMethod.ProviderPaymentMethodId;
                await UpsertPaymentMethodAsync(attempt.StudentId, paymentMethod, cancellationToken);
            }
        }

        var accessResult = await _courseAccessProvisioningService.GrantAccessAsync(
            attempt.CourseId,
            attempt.StudentId,
            attempt.StudentName,
            cancellationToken);

        if (accessResult.IsFailure)
            throw new InvalidOperationException(accessResult.Error);
    }

    private async Task HandleSubscriptionWebhookAsync(
        StripeWebhookEvent webhook,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(webhook.ProviderSubscriptionId))
            return;

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(x => x.ProviderSubscriptionId == webhook.ProviderSubscriptionId, cancellationToken);

        Guid? subscriptionPlanId = null;
        if (webhook.Metadata.TryGetValue("subscriptionPlanId", out var planIdRaw)
            && Guid.TryParse(planIdRaw, out var parsedPlanId))
        {
            subscriptionPlanId = parsedPlanId;
        }
        else if (subscription != null)
        {
            subscriptionPlanId = subscription.SubscriptionPlanId;
        }

        if (!subscriptionPlanId.HasValue)
            return;

        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(x => x.Id == subscriptionPlanId.Value, cancellationToken);
        if (plan == null)
            return;

        var studentId = webhook.Metadata.TryGetValue("studentId", out var studentIdRaw)
            ? studentIdRaw
            : subscription?.UserId;
        var providerCustomerId = webhook.CustomerId ?? subscription?.ProviderCustomerId;
        if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(providerCustomerId))
            return;

        if (subscription == null)
        {
            subscription = new UserSubscription
            {
                SubscriptionPlanId = plan.Id,
                UserId = studentId,
                Provider = _paymentsOptions.Provider,
                ProviderCustomerId = providerCustomerId,
                ProviderSubscriptionId = webhook.ProviderSubscriptionId,
                StartedAt = DateTime.UtcNow,
            };

            _context.UserSubscriptions.Add(subscription);
        }

        subscription.SubscriptionPlanId = plan.Id;
        subscription.UserId = studentId;
        subscription.ProviderCustomerId = providerCustomerId;
        subscription.ProviderSubscriptionId = webhook.ProviderSubscriptionId;
        subscription.PlanName = plan.Name;
        subscription.Price = plan.Price;
        subscription.Currency = plan.Currency;
        subscription.Status = ResolveUserSubscriptionStatus(webhook.SubscriptionStatus);
        subscription.CurrentPeriodStart = webhook.CurrentPeriodStart;
        subscription.CurrentPeriodEnd = webhook.CurrentPeriodEnd;
        subscription.CancelAtPeriodEnd = webhook.CancelAtPeriodEnd ?? false;
        subscription.CanceledAt = webhook.SubscriptionCanceledAt;
        subscription.PastDueSinceUtc = subscription.Status is UserSubscriptionStatus.PastDue or UserSubscriptionStatus.Unpaid
            ? subscription.PastDueSinceUtc ?? DateTime.UtcNow
            : null;
        subscription.StartedAt = subscription.StartedAt == default
            ? (webhook.CurrentPeriodStart ?? DateTime.UtcNow)
            : subscription.StartedAt;
        subscription.EndedAt = subscription.Status == UserSubscriptionStatus.Canceled
            ? (subscription.CanceledAt ?? webhook.CurrentPeriodEnd ?? DateTime.UtcNow)
            : null;

        var attempts = await _context.SubscriptionPaymentAttempts
            .Where(x => x.ProviderSubscriptionId == webhook.ProviderSubscriptionId
                     || (x.SubscriptionPlanId == plan.Id
                         && x.UserId == studentId
                         && x.Status == SubscriptionPaymentAttemptStatus.PendingProvider))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        foreach (var attempt in attempts)
        {
            attempt.ProviderSubscriptionId ??= webhook.ProviderSubscriptionId;
            attempt.ProviderCustomerId ??= providerCustomerId;

            if (subscription.Status is UserSubscriptionStatus.Active or UserSubscriptionStatus.Trialing)
            {
                attempt.Status = SubscriptionPaymentAttemptStatus.Succeeded;
                attempt.CompletedAt ??= DateTime.UtcNow;
                attempt.FailureMessage = null;
            }
            else if (subscription.Status is UserSubscriptionStatus.Incomplete or UserSubscriptionStatus.PastDue or UserSubscriptionStatus.Unpaid)
            {
                attempt.Status = SubscriptionPaymentAttemptStatus.Failed;
                attempt.FailureMessage = "Провайдер не активировал подписку.";
            }
            else if (subscription.Status == UserSubscriptionStatus.Canceled && attempt.Status != SubscriptionPaymentAttemptStatus.Succeeded)
            {
                attempt.Status = SubscriptionPaymentAttemptStatus.Canceled;
                attempt.FailureMessage = "Подписка была отменена у провайдера.";
            }
        }
    }

    private async Task HandleSubscriptionInvoiceWebhookAsync(
        StripeWebhookEvent webhook,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(webhook.ProviderInvoiceId)
            || string.IsNullOrWhiteSpace(webhook.ProviderSubscriptionId))
        {
            return;
        }

        var subscription = await _context.UserSubscriptions
            .FirstOrDefaultAsync(x => x.ProviderSubscriptionId == webhook.ProviderSubscriptionId, cancellationToken);

        Guid? subscriptionPlanId = null;
        if (webhook.Metadata.TryGetValue("subscriptionPlanId", out var planIdRaw)
            && Guid.TryParse(planIdRaw, out var parsedPlanId))
        {
            subscriptionPlanId = parsedPlanId;
        }
        else if (subscription != null)
        {
            subscriptionPlanId = subscription.SubscriptionPlanId;
        }

        if (!subscriptionPlanId.HasValue)
            return;

        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(x => x.Id == subscriptionPlanId.Value, cancellationToken);
        if (plan == null)
            return;

        var studentId = webhook.Metadata.TryGetValue("studentId", out var studentIdRaw)
            ? studentIdRaw
            : subscription?.UserId;
        var providerCustomerId = webhook.CustomerId ?? subscription?.ProviderCustomerId;
        if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(providerCustomerId))
            return;

        if (subscription == null)
        {
            subscription = new UserSubscription
            {
                SubscriptionPlanId = plan.Id,
                UserId = studentId,
                Provider = _paymentsOptions.Provider,
                ProviderCustomerId = providerCustomerId,
                ProviderSubscriptionId = webhook.ProviderSubscriptionId,
                StartedAt = webhook.CurrentPeriodStart ?? DateTime.UtcNow,
            };

            _context.UserSubscriptions.Add(subscription);
        }

        subscription.SubscriptionPlanId = plan.Id;
        subscription.UserId = studentId;
        subscription.ProviderCustomerId = providerCustomerId;
        subscription.ProviderSubscriptionId = webhook.ProviderSubscriptionId;
        subscription.PlanName = plan.Name;
        subscription.Price = plan.Price;
        subscription.Currency = plan.Currency;
        subscription.CurrentPeriodStart = webhook.CurrentPeriodStart ?? subscription.CurrentPeriodStart;
        subscription.CurrentPeriodEnd = webhook.CurrentPeriodEnd ?? subscription.CurrentPeriodEnd;
        subscription.StartedAt = subscription.StartedAt == default
            ? (subscription.CurrentPeriodStart ?? DateTime.UtcNow)
            : subscription.StartedAt;

        var invoice = await _context.SubscriptionInvoices
            .FirstOrDefaultAsync(x => x.ProviderInvoiceId == webhook.ProviderInvoiceId, cancellationToken);

        if (invoice == null)
        {
            invoice = new SubscriptionInvoice
            {
                Provider = _paymentsOptions.Provider,
                ProviderInvoiceId = webhook.ProviderInvoiceId,
            };

            _context.SubscriptionInvoices.Add(invoice);
        }

        var invoiceStatus = ResolveSubscriptionInvoiceStatus(webhook.InvoiceStatus, webhook.EventType);
        invoice.SubscriptionPlanId = plan.Id;
        invoice.UserSubscriptionId = subscription.Id;
        invoice.UserId = studentId;
        invoice.Provider = _paymentsOptions.Provider;
        invoice.ProviderInvoiceId = webhook.ProviderInvoiceId;
        invoice.ProviderSubscriptionId = webhook.ProviderSubscriptionId;
        invoice.PlanName = plan.Name;
        invoice.AmountDue = FromMinorUnits(webhook.AmountDueMinor ?? webhook.AmountMinor ?? 0);
        invoice.AmountPaid = FromMinorUnits(webhook.AmountPaidMinor ?? 0);
        invoice.Currency = (webhook.Currency ?? plan.Currency).Trim().ToLowerInvariant();
        invoice.Status = invoiceStatus;
        invoice.BillingReason = webhook.InvoiceBillingReason;
        invoice.PeriodStart = webhook.CurrentPeriodStart ?? subscription.CurrentPeriodStart;
        invoice.PeriodEnd = webhook.CurrentPeriodEnd ?? subscription.CurrentPeriodEnd;
        invoice.DueDate = webhook.InvoiceDueDate;
        invoice.PaidAt = webhook.InvoicePaidAt;
        invoice.FailureMessage = webhook.FailureMessage;
        invoice.UpdatedAt = DateTime.UtcNow;

        if (invoiceStatus == SubscriptionInvoiceStatus.Paid)
        {
            if (subscription.Status != UserSubscriptionStatus.Canceled)
            {
                subscription.Status = UserSubscriptionStatus.Active;
                subscription.PastDueSinceUtc = null;
                subscription.EndedAt = null;
            }
        }
        else if (invoiceStatus == SubscriptionInvoiceStatus.Failed)
        {
            if (subscription.Status != UserSubscriptionStatus.Canceled)
            {
                subscription.Status = subscription.Status is UserSubscriptionStatus.PendingActivation or UserSubscriptionStatus.Incomplete
                    ? UserSubscriptionStatus.Incomplete
                    : UserSubscriptionStatus.PastDue;
                if (subscription.Status == UserSubscriptionStatus.PastDue)
                    subscription.PastDueSinceUtc ??= DateTime.UtcNow;
            }
        }

        var attempts = await _context.SubscriptionPaymentAttempts
            .Where(x => x.ProviderSubscriptionId == webhook.ProviderSubscriptionId
                     || (x.SubscriptionPlanId == plan.Id
                         && x.UserId == studentId
                         && x.Status == SubscriptionPaymentAttemptStatus.PendingProvider))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        foreach (var attempt in attempts)
        {
            attempt.ProviderSubscriptionId ??= webhook.ProviderSubscriptionId;
            attempt.ProviderCustomerId ??= providerCustomerId;

            if (invoiceStatus == SubscriptionInvoiceStatus.Paid)
            {
                attempt.Status = SubscriptionPaymentAttemptStatus.Succeeded;
                attempt.CompletedAt ??= webhook.InvoicePaidAt ?? DateTime.UtcNow;
                attempt.FailureMessage = null;
            }
            else if (invoiceStatus is SubscriptionInvoiceStatus.Failed
                     or SubscriptionInvoiceStatus.Void
                     or SubscriptionInvoiceStatus.Uncollectible)
            {
                if (attempt.Status != SubscriptionPaymentAttemptStatus.Succeeded)
                {
                    attempt.Status = SubscriptionPaymentAttemptStatus.Failed;
                    attempt.FailureMessage = webhook.FailureMessage
                        ?? "Провайдер не подтвердил оплату инвойса подписки.";
                }
            }
        }

    }

    private async Task HandleCheckoutExpiredAsync(StripeWebhookEvent webhook, CancellationToken cancellationToken)
    {
        if (webhook.Metadata.ContainsKey("subscriptionPaymentAttemptId"))
        {
            await HandleSubscriptionCheckoutExpiredAsync(webhook, cancellationToken);
            return;
        }

        if (!webhook.Metadata.TryGetValue("paymentAttemptId", out var paymentAttemptIdRaw)
            || !Guid.TryParse(paymentAttemptIdRaw, out var paymentAttemptId))
        {
            return;
        }

        var attempt = await _context.PaymentAttempts
            .FirstOrDefaultAsync(x => x.Id == paymentAttemptId, cancellationToken);
        if (attempt == null || attempt.Status == PaymentAttemptStatus.Succeeded)
            return;

        attempt.Status = PaymentAttemptStatus.Expired;
        attempt.FailureMessage = "Сессия оплаты истекла.";
    }

    private async Task HandlePaymentIntentFailedAsync(StripeWebhookEvent webhook, CancellationToken cancellationToken)
    {
        var attempt = await FindCoursePaymentAttemptAsync(webhook, cancellationToken);
        if (attempt == null || attempt.Status == PaymentAttemptStatus.Succeeded)
            return;

        ApplyCourseAttemptWebhookData(attempt, webhook);
        attempt.Status = PaymentAttemptStatus.Failed;
        attempt.FailureMessage = webhook.FailureMessage ?? "Платёж не был завершён.";
    }

    private async Task HandleTransferCreatedAsync(StripeWebhookEvent webhook, CancellationToken cancellationToken)
    {
        var payoutRecord = await FindPayoutRecordForTransferAsync(webhook, cancellationToken);
        if (payoutRecord == null)
            return;

        payoutRecord.ProviderTransferId ??= webhook.ProviderTransferId;
        payoutRecord.Status = PayoutRecordStatus.Paid;
        payoutRecord.SubmittedAt ??= DateTime.UtcNow;
        payoutRecord.PaidAt ??= DateTime.UtcNow;
        payoutRecord.FailureMessage = null;

        var settlements = await _context.TeacherSettlements
            .Where(x => x.PayoutRecordId == payoutRecord.Id)
            .ToListAsync(cancellationToken);

        foreach (var settlement in settlements)
            await RecalculateSettlementStatusAsync(settlement, cancellationToken);
    }

    private async Task HandleTransferReversedAsync(StripeWebhookEvent webhook, CancellationToken cancellationToken)
    {
        var payoutRecord = await FindPayoutRecordForTransferAsync(webhook, cancellationToken);
        if (payoutRecord == null)
            return;

        payoutRecord.Status = PayoutRecordStatus.Reversed;
        payoutRecord.FailureMessage = webhook.FailureMessage ?? "Transfer был сторнирован провайдером.";

        var settlements = await _context.TeacherSettlements
            .Where(x => x.PayoutRecordId == payoutRecord.Id)
            .ToListAsync(cancellationToken);

        foreach (var settlement in settlements)
            await RecalculateSettlementStatusAsync(settlement, cancellationToken);
    }

    private async Task<PayoutRecord?> FindPayoutRecordForTransferAsync(
        StripeWebhookEvent webhook,
        CancellationToken cancellationToken)
    {
        PayoutRecord? payoutRecord = null;

        if (!string.IsNullOrWhiteSpace(webhook.ProviderTransferId))
        {
            payoutRecord = await _context.PayoutRecords
                .FirstOrDefaultAsync(x => x.ProviderTransferId == webhook.ProviderTransferId, cancellationToken);
        }

        if (payoutRecord != null)
            return payoutRecord;

        if (webhook.Metadata.TryGetValue("payoutRecordId", out var payoutRecordIdValue)
            && Guid.TryParse(payoutRecordIdValue, out var payoutRecordId))
        {
            payoutRecord = await _context.PayoutRecords
                .FirstOrDefaultAsync(x => x.Id == payoutRecordId, cancellationToken);

            if (payoutRecord != null && string.IsNullOrWhiteSpace(payoutRecord.ProviderTransferId))
                payoutRecord.ProviderTransferId = webhook.ProviderTransferId;
        }

        return payoutRecord;
    }

    private async Task RecalculateSettlementStatusAsync(
        TeacherSettlement settlement,
        CancellationToken cancellationToken)
    {
        PayoutRecord? payoutRecord = null;
        if (settlement.PayoutRecordId != null)
        {
            payoutRecord = await _context.PayoutRecords
                .FirstOrDefaultAsync(x => x.Id == settlement.PayoutRecordId.Value, cancellationToken);
        }

        if (GetRemainingNetAmount(settlement) <= 0)
        {
            settlement.Status = settlement.PaidOutAt.HasValue || payoutRecord?.Status == PayoutRecordStatus.Paid
                ? TeacherSettlementStatus.Reversed
                : TeacherSettlementStatus.Canceled;
            return;
        }

        if (payoutRecord != null)
        {
            if (payoutRecord.Status is PayoutRecordStatus.Canceled or PayoutRecordStatus.Failed or PayoutRecordStatus.Reversed)
            {
                settlement.PayoutRecordId = null;
                if (payoutRecord.Status != PayoutRecordStatus.Paid)
                    settlement.PaidOutAt = null;

                payoutRecord = null;
            }
        }

        if (payoutRecord != null)
        {
            settlement.Status = payoutRecord.Status switch
            {
                PayoutRecordStatus.Paid => TeacherSettlementStatus.PaidOut,
                PayoutRecordStatus.Queued or PayoutRecordStatus.SubmittedToProvider => TeacherSettlementStatus.InPayout,
                _ => settlement.AvailableAt <= DateTime.UtcNow
                    ? TeacherSettlementStatus.ReadyForPayout
                    : TeacherSettlementStatus.PendingHold,
            };

            if (payoutRecord.Status == PayoutRecordStatus.Paid)
                settlement.PaidOutAt ??= payoutRecord.PaidAt;

            return;
        }

        settlement.Status = settlement.AvailableAt <= DateTime.UtcNow
            ? TeacherSettlementStatus.ReadyForPayout
            : TeacherSettlementStatus.PendingHold;
    }

    private async Task UpsertPaymentMethodAsync(
        string studentId,
        ProviderPaymentMethodSnapshot paymentMethod,
        CancellationToken cancellationToken)
    {
        await ClearOtherDefaultPaymentMethodsAsync(
            studentId,
            paymentMethod.ProviderPaymentMethodId,
            cancellationToken);

        var existing = await _context.PaymentMethods
            .FirstOrDefaultAsync(x => x.ProviderPaymentMethodId == paymentMethod.ProviderPaymentMethodId, cancellationToken);

        if (existing == null)
        {
            _context.PaymentMethods.Add(new PaymentMethodRef
            {
                UserId = studentId,
                Provider = _paymentsOptions.Provider,
                ProviderCustomerId = paymentMethod.ProviderCustomerId,
                ProviderPaymentMethodId = paymentMethod.ProviderPaymentMethodId,
                Brand = paymentMethod.Brand,
                Last4 = paymentMethod.Last4,
                ExpMonth = paymentMethod.ExpMonth,
                ExpYear = paymentMethod.ExpYear,
                IsDefault = true,
            });
            return;
        }

        existing.UserId = studentId;
        existing.ProviderCustomerId = paymentMethod.ProviderCustomerId;
        existing.Brand = paymentMethod.Brand;
        existing.Last4 = paymentMethod.Last4;
        existing.ExpMonth = paymentMethod.ExpMonth;
        existing.ExpYear = paymentMethod.ExpYear;
        existing.IsDefault = true;
        existing.RemovedAt = null;
    }

    private async Task<UserPaymentProfile> GetOrCreatePaymentProfileAsync(
        string studentId,
        string studentEmail,
        string studentName,
        CancellationToken cancellationToken)
    {
        var paymentProfile = await _context.UserPaymentProfiles
            .FirstOrDefaultAsync(x => x.UserId == studentId, cancellationToken);

        if (paymentProfile != null)
            return paymentProfile;

        var providerCustomerId = await _gateway.CreateCustomerAsync(
            studentId,
            studentEmail,
            studentName,
            cancellationToken);

        paymentProfile = new UserPaymentProfile
        {
            UserId = studentId,
            Provider = _paymentsOptions.Provider,
            ProviderCustomerId = providerCustomerId,
        };

        _context.UserPaymentProfiles.Add(paymentProfile);
        await _context.SaveChangesAsync(cancellationToken);
        return paymentProfile;
    }

    private async Task ClearOtherDefaultPaymentMethodsAsync(
        string studentId,
        string providerPaymentMethodId,
        CancellationToken cancellationToken)
    {
        var otherMethods = await _context.PaymentMethods
            .Where(x => x.UserId == studentId
                     && x.RemovedAt == null
                     && x.ProviderPaymentMethodId != providerPaymentMethodId
                     && x.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var otherMethod in otherMethods)
            otherMethod.IsDefault = false;
    }

    private async Task NormalizeActivePaymentMethodDefaultsAsync(
        string studentId,
        CancellationToken cancellationToken)
    {
        var activeMethods = await _context.PaymentMethods
            .Where(x => x.UserId == studentId && x.RemovedAt == null)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        if (activeMethods.Count == 0)
            return;

        var defaultMethodId = activeMethods.FirstOrDefault(x => x.IsDefault)?.Id ?? activeMethods[0].Id;
        foreach (var activeMethod in activeMethods)
            activeMethod.IsDefault = activeMethod.Id == defaultMethodId;
    }

    private async Task EnsureTeacherSettlementExistsAsync(
        PaymentAttempt attempt,
        CoursePurchase purchase,
        decimal providerFeeAmount,
        CancellationToken cancellationToken)
    {
        var exists = await _context.TeacherSettlements
            .AnyAsync(x => x.PaymentAttemptId == attempt.Id, cancellationToken);
        if (exists)
            return;

        var grossAmount = attempt.Amount;
        var platformCommissionAmount = CalculatePlatformCommissionAmount(grossAmount);
        var netAmount = Math.Max(0m, grossAmount - providerFeeAmount - platformCommissionAmount);
        var availableAt = DateTime.UtcNow.AddDays(GetSettlementHoldDays());
        var status = availableAt <= DateTime.UtcNow
            ? TeacherSettlementStatus.ReadyForPayout
            : TeacherSettlementStatus.PendingHold;

        _context.TeacherSettlements.Add(new TeacherSettlement
        {
            TeacherId = attempt.TeacherId,
            CourseId = attempt.CourseId,
            CourseTitle = attempt.CourseTitle,
            StudentId = attempt.StudentId,
            StudentName = attempt.StudentName,
            PaymentAttemptId = attempt.Id,
            CoursePurchaseId = purchase.Id,
            GrossAmount = grossAmount,
            ProviderFeeAmount = providerFeeAmount,
            PlatformCommissionAmount = platformCommissionAmount,
            NetAmount = netAmount,
            Currency = attempt.Currency,
            Status = status,
            AvailableAt = availableAt,
        });
    }

    private async Task RefreshTeacherSettlementsAsync(
        string teacherId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var dueSettlements = await _context.TeacherSettlements
            .Where(x => x.TeacherId == teacherId
                     && x.Status == TeacherSettlementStatus.PendingHold
                     && x.AvailableAt <= now)
            .ToListAsync(cancellationToken);

        if (dueSettlements.Count == 0)
            return;

        foreach (var settlement in dueSettlements)
            await RecalculateSettlementStatusAsync(settlement, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private decimal CalculatePlatformCommissionAmount(decimal grossAmount)
    {
        if (_paymentsOptions.PlatformCommissionPercent <= 0)
            return 0m;

        var rawAmount = grossAmount * (_paymentsOptions.PlatformCommissionPercent / 100m);
        return Math.Round(rawAmount, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal GetRemainingNetAmount(TeacherSettlement settlement)
    {
        return Math.Max(0m, settlement.NetAmount);
    }

    private static decimal FromMinorUnits(long amountMinor)
    {
        return decimal.Round(amountMinor / 100m, 2, MidpointRounding.AwayFromZero);
    }

    private static long ToMinorUnits(decimal amount)
    {
        return decimal.ToInt64(decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));
    }

    private static UserSubscriptionStatus ResolveUserSubscriptionStatus(string? providerStatus)
    {
        return providerStatus?.Trim().ToLowerInvariant() switch
        {
            "active" => UserSubscriptionStatus.Active,
            "trialing" => UserSubscriptionStatus.Trialing,
            "incomplete" or "incomplete_expired" => UserSubscriptionStatus.Incomplete,
            "past_due" => UserSubscriptionStatus.PastDue,
            "unpaid" => UserSubscriptionStatus.Unpaid,
            "paused" => UserSubscriptionStatus.Paused,
            "canceled" => UserSubscriptionStatus.Canceled,
            _ => UserSubscriptionStatus.PendingActivation,
        };
    }

    private static SubscriptionInvoiceStatus ResolveSubscriptionInvoiceStatus(string? providerStatus, string eventType)
    {
        return eventType.Trim().ToLowerInvariant() switch
        {
            "invoice.paid" => SubscriptionInvoiceStatus.Paid,
            "invoice.payment_failed" => SubscriptionInvoiceStatus.Failed,
            "invoice.voided" => SubscriptionInvoiceStatus.Void,
            "invoice.marked_uncollectible" => SubscriptionInvoiceStatus.Uncollectible,
            _ => providerStatus?.Trim().ToLowerInvariant() switch
            {
                "paid" => SubscriptionInvoiceStatus.Paid,
                "uncollectible" => SubscriptionInvoiceStatus.Uncollectible,
                "void" => SubscriptionInvoiceStatus.Void,
                "open" or "draft" => SubscriptionInvoiceStatus.Open,
                _ => SubscriptionInvoiceStatus.Open,
            },
        };
    }

    private static Expression<Func<SubscriptionPlan, SubscriptionPlanDto>> MapSubscriptionPlanProjection()
    {
        return x => new SubscriptionPlanDto(
            x.Id,
            x.Name,
            x.Description,
            x.Price,
            x.Currency,
            x.BillingInterval.ToString(),
            x.BillingIntervalCount,
            x.IsActive,
            x.IsFeatured,
            x.SortOrder,
            x.ProviderProductId,
            x.ProviderPriceId,
            x.IndividualSlotsPerMonth,
            x.GroupSlotsPerMonth,
            x.CreatedAt,
            x.UpdatedAt);
    }

    private static SubscriptionPlanDto MapSubscriptionPlan(SubscriptionPlan plan)
    {
        return new SubscriptionPlanDto(
            plan.Id,
            plan.Name,
            plan.Description,
            plan.Price,
            plan.Currency,
            plan.BillingInterval.ToString(),
            plan.BillingIntervalCount,
            plan.IsActive,
            plan.IsFeatured,
            plan.SortOrder,
            plan.ProviderProductId,
            plan.ProviderPriceId,
            plan.IndividualSlotsPerMonth,
            plan.GroupSlotsPerMonth,
            plan.CreatedAt,
            plan.UpdatedAt);
    }

    private static void ApplySubscriptionPlanInput(
        SubscriptionPlan plan,
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
        int individualSlotsPerMonth,
        int groupSlotsPerMonth)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Название тарифа обязательно.");
        if (price <= 0)
            throw new InvalidOperationException("Стоимость тарифа должна быть больше нуля.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidOperationException("Валюта тарифа обязательна.");
        if (billingIntervalCount <= 0)
            throw new InvalidOperationException("Интервал тарифа должен быть больше нуля.");
        if (individualSlotsPerMonth < 0)
            throw new InvalidOperationException("Количество индивидуальных занятий не может быть отрицательным.");
        if (groupSlotsPerMonth < 0)
            throw new InvalidOperationException("Количество групповых занятий не может быть отрицательным.");

        if (!Enum.TryParse<SubscriptionBillingInterval>(billingInterval, true, out var parsedInterval))
            throw new InvalidOperationException("Некорректный billing interval тарифа.");

        plan.Name = name.Trim();
        plan.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        plan.Price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
        plan.Currency = currency.Trim().ToLowerInvariant();
        plan.BillingInterval = parsedInterval;
        plan.BillingIntervalCount = billingIntervalCount;
        plan.IsActive = isActive;
        plan.IsFeatured = isFeatured;
        plan.SortOrder = sortOrder;
        plan.ProviderProductId = string.IsNullOrWhiteSpace(providerProductId) ? null : providerProductId.Trim();
        plan.ProviderPriceId = string.IsNullOrWhiteSpace(providerPriceId) ? null : providerPriceId.Trim();
        plan.IndividualSlotsPerMonth = individualSlotsPerMonth;
        plan.GroupSlotsPerMonth = groupSlotsPerMonth;
    }

    private static Expression<Func<PaymentAttempt, PaymentAttemptDto>> MapPaymentAttemptProjection()
    {
        return x => new PaymentAttemptDto(
            x.Id,
            x.CourseId,
            x.CourseTitle,
            x.Amount,
            x.Currency,
            x.Status.ToString(),
            x.ProviderChargeId,
            x.FailureMessage,
            x.CreatedAt,
            x.CompletedAt);
    }

    private static PaymentAttemptDto MapPaymentAttempt(PaymentAttempt attempt)
    {
        return new PaymentAttemptDto(
            attempt.Id,
            attempt.CourseId,
            attempt.CourseTitle,
            attempt.Amount,
            attempt.Currency,
            attempt.Status.ToString(),
            attempt.ProviderChargeId,
            attempt.FailureMessage,
            attempt.CreatedAt,
            attempt.CompletedAt);
    }

    private static PayoutRecordDto MapPayoutRecord(PayoutRecord record)
    {
        return new PayoutRecordDto(
            record.Id,
            record.Amount,
            record.Currency,
            record.SettlementsCount,
            record.Status.ToString(),
            record.ProviderTransferId,
            record.RequestedAt,
            record.SubmittedAt,
            record.PaidAt,
            record.FailedAt,
            record.FailureMessage);
    }

    private static TeacherPayoutAccountDto MapTeacherPayoutAccount(
        TeacherPayoutAccount? account,
        bool providerConfigured)
    {
        if (account == null)
        {
            return new TeacherPayoutAccountDto(
                TeacherPayoutAccountStatus.NotStarted.ToString(),
                providerConfigured,
                false,
                false,
                false,
                false,
                providerConfigured ? null : "Платёжный провайдер не настроен.");
        }

        return new TeacherPayoutAccountDto(
            account.Status.ToString(),
            providerConfigured,
            account.ChargesEnabled,
            account.PayoutsEnabled,
            account.DetailsSubmitted,
            providerConfigured && account.Status == TeacherPayoutAccountStatus.Ready,
            account.RequirementsSummary);
    }

    private static void ApplyProviderSnapshot(TeacherPayoutAccount account, ProviderTeacherAccountResult snapshot)
    {
        account.ProviderAccountId = snapshot.ProviderAccountId;
        account.ChargesEnabled = snapshot.ChargesEnabled;
        account.PayoutsEnabled = snapshot.PayoutsEnabled;
        account.DetailsSubmitted = snapshot.DetailsSubmitted;
        account.RequirementsSummary = snapshot.RequirementsSummary;
        account.Status = ResolveTeacherPayoutStatus(snapshot);

        if (account.Status == TeacherPayoutAccountStatus.Ready)
            account.ReadyAt ??= DateTime.UtcNow;
        else
            account.OnboardingStartedAt ??= DateTime.UtcNow;
    }

    private static TeacherPayoutAccountStatus ResolveTeacherPayoutStatus(ProviderTeacherAccountResult snapshot)
    {
        if (snapshot.ChargesEnabled && snapshot.PayoutsEnabled)
            return TeacherPayoutAccountStatus.Ready;

        if (snapshot.DetailsSubmitted)
        {
            return string.IsNullOrWhiteSpace(snapshot.RequirementsSummary)
                ? TeacherPayoutAccountStatus.PendingVerification
                : TeacherPayoutAccountStatus.Restricted;
        }

        return TeacherPayoutAccountStatus.OnboardingStarted;
    }

    private void EnsureProviderConfigured()
    {
        if (!_gateway.IsConfigured)
            throw new InvalidOperationException("Платёжный провайдер не настроен.");
    }

    private bool IsLocalProvider()
    {
        return string.Equals(_paymentsOptions.Provider, "Local", StringComparison.OrdinalIgnoreCase)
            || string.Equals(_paymentsOptions.Provider, "Mock", StringComparison.OrdinalIgnoreCase)
            || string.Equals(_paymentsOptions.Provider, "Development", StringComparison.OrdinalIgnoreCase);
    }

    private int GetSettlementHoldDays()
    {
        return IsLocalProvider()
            ? 0
            : Math.Max(0, _paymentsOptions.SettlementHoldDays);
    }
}

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

public class PaymentsService : IPaymentsService, ITeacherPayoutReadService
{
    private readonly IPaymentsDbContext _context;
    private readonly IPaymentProviderGateway _gateway;
    private readonly ICoursePaymentReadService _coursePaymentReadService;
    private readonly IEnrollmentReadService _enrollmentReadService;
    private readonly ISubscriptionAllocationReadService _subscriptionAllocationReadService;
    private readonly ICourseAccessProvisioningService _courseAccessProvisioningService;
    private readonly ICourseAccessRevocationService _courseAccessRevocationService;
    private readonly PaymentsOptions _paymentsOptions;
    private readonly IConfiguration _configuration;

    public PaymentsService(
        IPaymentsDbContext context,
        IPaymentProviderGateway gateway,
        ICoursePaymentReadService coursePaymentReadService,
        IEnrollmentReadService enrollmentReadService,
        ISubscriptionAllocationReadService subscriptionAllocationReadService,
        ICourseAccessProvisioningService courseAccessProvisioningService,
        ICourseAccessRevocationService courseAccessRevocationService,
        IOptions<PaymentsOptions> paymentsOptions,
        IConfiguration configuration)
    {
        _context = context;
        _gateway = gateway;
        _coursePaymentReadService = coursePaymentReadService;
        _enrollmentReadService = enrollmentReadService;
        _subscriptionAllocationReadService = subscriptionAllocationReadService;
        _courseAccessProvisioningService = courseAccessProvisioningService;
        _courseAccessRevocationService = courseAccessRevocationService;
        _paymentsOptions = paymentsOptions.Value;
        _configuration = configuration;
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

        var now = DateTime.UtcNow;
        var settlements = await _context.TeacherSettlements
            .Where(x => x.TeacherId == teacherId)
            .ToListAsync(cancellationToken);
        var allocationRows = await (from line in _context.SubscriptionAllocationLines
                                    join run in _context.SubscriptionAllocationRuns
                                      on line.SubscriptionAllocationRunId equals run.Id
                                    join payout in _context.PayoutRecords
                                      on line.PayoutRecordId equals payout.Id into payoutGroup
                                    from payout in payoutGroup.DefaultIfEmpty()
                                    where line.TeacherId == teacherId
                                    select new
                                    {
                                        line,
                                        RunStatus = run.Status,
                                        PayoutStatus = payout != null ? (PayoutRecordStatus?)payout.Status : null,
                                    })
            .ToListAsync(cancellationToken);

        var activeAllocationRows = allocationRows
            .Where(x => x.RunStatus == SubscriptionAllocationRunStatus.Applied)
            .ToList();

        var pendingAllocationNetAmount = activeAllocationRows
            .Where(x => x.line.PayoutRecordId == null && x.line.AvailableAt > now)
            .Sum(x => x.line.NetAmount);
        var readyAllocationNetAmount = activeAllocationRows
            .Where(x => x.line.PayoutRecordId == null && x.line.AvailableAt <= now)
            .Sum(x => x.line.NetAmount);
        var inPayoutAllocationNetAmount = activeAllocationRows
            .Where(x => x.line.PayoutRecordId != null
                     && x.PayoutStatus is PayoutRecordStatus.Queued or PayoutRecordStatus.SubmittedToProvider)
            .Sum(x => x.line.NetAmount);
        var paidAllocationNetAmount = activeAllocationRows
            .Where(x => x.line.PayoutRecordId != null && x.PayoutStatus == PayoutRecordStatus.Paid)
            .Sum(x => x.line.NetAmount);
        var totalAllocationGrossAmount = activeAllocationRows.Sum(x => x.line.GrossAmount);
        var totalAllocationNetAmount = activeAllocationRows.Sum(x => x.line.NetAmount);

        return new TeacherSettlementSummaryDto(
            settlements.Sum(x => x.GrossAmount) + totalAllocationGrossAmount,
            settlements.Sum(GetRemainingNetAmount) + totalAllocationNetAmount,
            settlements
                .Where(x => x.Status == TeacherSettlementStatus.PendingHold)
                .Sum(GetRemainingNetAmount) + pendingAllocationNetAmount,
            settlements
                .Where(x => x.Status == TeacherSettlementStatus.ReadyForPayout)
                .Sum(GetRemainingNetAmount) + readyAllocationNetAmount,
            settlements
                .Where(x => x.Status == TeacherSettlementStatus.InPayout)
                .Sum(GetRemainingNetAmount) + inPayoutAllocationNetAmount,
            settlements
                .Where(x => x.Status == TeacherSettlementStatus.PaidOut)
                .Sum(GetRemainingNetAmount) + paidAllocationNetAmount,
            settlements.Sum(x => x.RefundedNetAmount),
            settlements.Sum(x => x.DisputedNetAmount),
            settlements.Count + activeAllocationRows.Count,
            activeAllocationRows.Count,
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
                x.RefundedGrossAmount,
                x.RefundedNetAmount,
                x.DisputedGrossAmount,
                x.DisputedNetAmount,
                x.Currency,
                x.Status.ToString(),
                x.AvailableAt,
                x.PaidOutAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TeacherSubscriptionAllocationDto>> GetTeacherSubscriptionAllocationsAsync(
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var rows = await (from line in _context.SubscriptionAllocationLines
                          join run in _context.SubscriptionAllocationRuns
                            on line.SubscriptionAllocationRunId equals run.Id
                          join payout in _context.PayoutRecords
                            on line.PayoutRecordId equals payout.Id into payoutGroup
                          from payout in payoutGroup.DefaultIfEmpty()
                          where line.TeacherId == teacherId
                          orderby line.AllocatedAt descending
                          select new
                          {
                              line,
                              run.PlanName,
                              run.Status,
                              run.PeriodStart,
                              run.PeriodEnd,
                              PayoutStatus = payout != null ? (PayoutRecordStatus?)payout.Status : null,
                          })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new TeacherSubscriptionAllocationDto(
                row.line.Id,
                row.line.SubscriptionAllocationRunId,
                row.line.SubscriptionInvoiceId,
                row.line.SubscriptionPlanId,
                row.PlanName,
                row.line.CourseId,
                row.line.CourseTitle,
                row.line.AllocationWeight,
                row.line.ProgressPercent,
                row.line.CompletedLessons,
                row.line.TotalLessons,
                row.line.GrossAmount,
                row.line.PlatformCommissionAmount,
                row.line.ProviderFeeAmount,
                row.line.NetAmount,
                row.line.Currency,
                row.Status.ToString(),
                ResolveSubscriptionAllocationPayoutStatus(
                    row.Status,
                    row.line.AvailableAt,
                    row.PayoutStatus,
                    now),
                row.PeriodStart,
                row.PeriodEnd,
                row.line.AvailableAt,
                row.line.PaidOutAt,
                row.line.AllocatedAt))
            .ToList();
    }

    public async Task<IReadOnlyList<RefundRecordDto>> GetMyRefundsAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        return await (from refund in _context.RefundRecords
                      join attempt in _context.PaymentAttempts
                        on refund.PaymentAttemptId equals attempt.Id
                      where refund.StudentId == studentId
                      orderby refund.RequestedAt descending
                      select new RefundRecordDto(
                          refund.Id,
                          attempt.CourseId,
                          refund.CourseTitle,
                          refund.Amount,
                          refund.TeacherNetRefundAmount,
                          refund.Currency,
                          refund.Status.ToString(),
                          refund.Reason,
                          refund.FailureMessage,
                          refund.RequestedAt,
                          refund.ProcessedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DisputeRecordDto>> GetMyDisputesAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        return await (from dispute in _context.DisputeRecords
                      join attempt in _context.PaymentAttempts
                        on dispute.PaymentAttemptId equals attempt.Id
                      where dispute.StudentId == studentId
                      orderby dispute.OpenedAt descending
                      select new DisputeRecordDto(
                          dispute.Id,
                          attempt.CourseId,
                          dispute.CourseTitle,
                          dispute.Amount,
                          dispute.TeacherNetDisputeAmount,
                          dispute.Currency,
                          dispute.Status.ToString(),
                          dispute.Reason,
                          dispute.OpenedAt,
                          dispute.EvidenceDueBy,
                          dispute.FundsWithdrawnAt,
                          dispute.FundsReinstatedAt,
                          dispute.ClosedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DisputeRecordDto>> GetTeacherDisputesAsync(
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        return await (from dispute in _context.DisputeRecords
                      join attempt in _context.PaymentAttempts
                        on dispute.PaymentAttemptId equals attempt.Id
                      where dispute.TeacherId == teacherId
                      orderby dispute.OpenedAt descending
                      select new DisputeRecordDto(
                          dispute.Id,
                          attempt.CourseId,
                          dispute.CourseTitle,
                          dispute.Amount,
                          dispute.TeacherNetDisputeAmount,
                          dispute.Currency,
                          dispute.Status.ToString(),
                          dispute.Reason,
                          dispute.OpenedAt,
                          dispute.EvidenceDueBy,
                          dispute.FundsWithdrawnAt,
                          dispute.FundsReinstatedAt,
                          dispute.ClosedAt))
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
            providerPriceId);

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
            providerPriceId);

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
            let refundedAmount = _context.RefundRecords
                .Where(r => r.PaymentAttemptId == attempt.Id && r.Status == RefundRecordStatus.Succeeded)
                .Select(r => (decimal?)r.Amount)
                .Sum() ?? 0m
            let pendingRefundAmount = _context.RefundRecords
                .Where(r => r.PaymentAttemptId == attempt.Id && r.Status == RefundRecordStatus.Pending)
                .Select(r => (decimal?)r.Amount)
                .Sum() ?? 0m
            select new
            {
                attempt,
                purchase,
                RefundedAmount = refundedAmount,
                PendingRefundAmount = pendingRefundAmount,
            };

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

        var attemptIds = pageItems.Select(x => x.attempt.Id).ToArray();
        var disputes = await _context.DisputeRecords
            .Where(x => attemptIds.Contains(x.PaymentAttemptId))
            .OrderByDescending(x => x.OpenedAt)
            .ToListAsync(cancellationToken);

        var disputesByAttempt = disputes
            .GroupBy(x => x.PaymentAttemptId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var items = new List<AdminPaymentRecordDto>(pageItems.Count);
        foreach (var item in pageItems)
        {
            var courseInfo = await _coursePaymentReadService.GetCoursePaymentInfoAsync(
                item.attempt.CourseId,
                cancellationToken);
            disputesByAttempt.TryGetValue(item.attempt.Id, out var attemptDisputes);

            var remainingRefundableAmount = Math.Max(
                0m,
                item.attempt.Amount - item.RefundedAmount - item.PendingRefundAmount);
            var disputedAmount = attemptDisputes?
                .Where(x => x.LedgerAppliedAt != null && x.LedgerRestoredAt == null)
                .Sum(x => x.AppliedGrossAmount) ?? 0m;
            var latestDisputeStatus = attemptDisputes?.FirstOrDefault()?.Status.ToString();
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
                item.RefundedAmount,
                item.PendingRefundAmount,
                disputedAmount,
                remainingRefundableAmount,
                settlement?.ProviderFeeAmount ?? 0m,
                item.attempt.Currency,
                item.attempt.Status.ToString(),
                item.attempt.ProviderChargeId,
                latestDisputeStatus,
                item.purchase?.Status.ToString(),
                item.attempt.CreatedAt,
                item.attempt.CompletedAt));
        }

        return new PagedResult<AdminPaymentRecordDto>(items, totalCount, page, pageSize);
    }

    public async Task<RefundRecordDto> CreateAdminRefundAsync(
        Guid paymentAttemptId,
        decimal? amount,
        string? reason,
        string adminId,
        CancellationToken cancellationToken = default)
    {
        EnsureProviderConfigured();

        var attempt = await _context.PaymentAttempts
            .FirstOrDefaultAsync(x => x.Id == paymentAttemptId, cancellationToken);
        if (attempt == null)
            throw new InvalidOperationException("Платёж не найден.");

        if (string.IsNullOrWhiteSpace(attempt.ProviderPaymentIntentId))
            throw new InvalidOperationException("Для этого платежа нет provider payment intent.");

        if (attempt.Status is not PaymentAttemptStatus.Succeeded and not PaymentAttemptStatus.PartiallyRefunded)
            throw new InvalidOperationException("Возврат можно оформить только для успешного платежа.");

        var committedRefundAmount = await _context.RefundRecords
            .Where(x => x.PaymentAttemptId == paymentAttemptId
                     && x.Status != RefundRecordStatus.Failed
                     && x.Status != RefundRecordStatus.Canceled)
            .SumAsync(x => x.Amount, cancellationToken);

        var remainingRefundableAmount = Math.Max(0m, attempt.Amount - committedRefundAmount);
        if (remainingRefundableAmount <= 0)
            throw new InvalidOperationException("У этого платежа больше нет доступной суммы для refund.");

        var refundAmount = amount ?? remainingRefundableAmount;
        if (refundAmount <= 0)
            throw new InvalidOperationException("Сумма refund должна быть больше нуля.");
        if (refundAmount > remainingRefundableAmount)
            throw new InvalidOperationException("Сумма refund превышает доступный остаток платежа.");

        var providerRefund = await _gateway.CreateRefundAsync(
            new ProviderRefundRequest(
                attempt.ProviderPaymentIntentId,
                refundAmount,
                attempt.Currency,
                reason,
                attempt.Id,
                attempt.CourseId,
                attempt.TeacherId,
                attempt.StudentId,
                adminId),
            cancellationToken);

        var purchase = await _context.CoursePurchases
            .FirstOrDefaultAsync(x => x.PaymentAttemptId == attempt.Id, cancellationToken);
        var settlement = await _context.TeacherSettlements
            .FirstOrDefaultAsync(x => x.PaymentAttemptId == attempt.Id, cancellationToken);

        var refund = await UpsertRefundRecordAsync(
            attempt,
            purchase,
            settlement,
            providerRefund.ProviderRefundId,
            providerRefund.PaymentIntentId,
            providerRefund.Amount,
            providerRefund.Currency,
            ResolveRefundStatus(providerRefund.Status),
            providerRefund.Reason ?? reason,
            providerRefund.FailureMessage,
            adminId,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return MapRefundRecord(refund, attempt.CourseId);
    }

    public async Task<IReadOnlyList<AdminSubscriptionAllocationRunDto>> GetAdminSubscriptionAllocationRunsAsync(
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 100);

        var runs = await _context.SubscriptionAllocationRuns
            .OrderByDescending(x => x.AllocatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        if (runs.Count == 0)
            return [];

        var runIds = runs.Select(x => x.Id).ToArray();
        var lines = await _context.SubscriptionAllocationLines
            .Where(x => runIds.Contains(x.SubscriptionAllocationRunId))
            .OrderByDescending(x => x.NetAmount)
            .ThenBy(x => x.TeacherName)
            .ToListAsync(cancellationToken);

        var linesByRun = lines
            .GroupBy(x => x.SubscriptionAllocationRunId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<AdminSubscriptionAllocationLineDto>)x
                    .Select(line => new AdminSubscriptionAllocationLineDto(
                        line.Id,
                        line.TeacherId,
                        line.TeacherName,
                        line.CourseId,
                        line.CourseTitle,
                        line.AllocationWeight,
                        line.ProgressPercent,
                        line.CompletedLessons,
                        line.TotalLessons,
                        line.GrossAmount,
                        line.PlatformCommissionAmount,
                        line.ProviderFeeAmount,
                        line.NetAmount,
                        line.Currency))
                    .ToList());

        return runs
            .Select(run => new AdminSubscriptionAllocationRunDto(
                run.Id,
                run.SubscriptionInvoiceId,
                run.UserId,
                run.SubscriptionPlanId,
                run.PlanName,
                run.GrossAmount,
                run.PlatformCommissionAmount,
                run.ProviderFeeAmount,
                run.NetAmount,
                run.Currency,
                run.Strategy,
                run.Status.ToString(),
                run.TeacherCount,
                run.CourseCount,
                run.PeriodStart,
                run.PeriodEnd,
                run.AllocatedAt,
                linesByRun.TryGetValue(run.Id, out var runLines) ? runLines : []))
            .ToList();
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
                x.AllocationLinesCount,
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
        var now = DateTime.UtcNow;

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

        var payableAllocationLines = await (from line in _context.SubscriptionAllocationLines
                                            join run in _context.SubscriptionAllocationRuns
                                              on line.SubscriptionAllocationRunId equals run.Id
                                            where line.TeacherId == teacherId
                                               && run.Status == SubscriptionAllocationRunStatus.Applied
                                               && line.PayoutRecordId == null
                                               && line.AvailableAt <= now
                                               && line.NetAmount > 0
                                            orderby line.AvailableAt, line.AllocatedAt
                                            select line)
            .ToListAsync(cancellationToken);

        if (payableSettlements.Count == 0 && payableAllocationLines.Count == 0)
            throw new InvalidOperationException("Нет начислений, готовых к выплате.");

        var payoutCurrency = payableSettlements.FirstOrDefault()?.Currency
            ?? payableAllocationLines.First().Currency;

        var payoutRecord = new PayoutRecord
        {
            TeacherId = teacherId,
            Provider = _paymentsOptions.Provider,
            ProviderAccountId = payoutAccount.ProviderAccountId,
            Amount = payableSettlements.Sum(GetRemainingNetAmount) + payableAllocationLines.Sum(x => x.NetAmount),
            Currency = payoutCurrency,
            SettlementsCount = payableSettlements.Count,
            AllocationLinesCount = payableAllocationLines.Count,
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
                    payoutRecord.SettlementsCount + payoutRecord.AllocationLinesCount),
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

        foreach (var allocationLine in payableAllocationLines)
            allocationLine.PayoutRecordId = payoutRecord.Id;

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
                x.EndedAt))
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
            case "refund.created":
            case "refund.updated":
            case "refund.failed":
                await HandleRefundEventAsync(webhook, cancellationToken);
                break;
            case "customer.subscription.created":
            case "customer.subscription.updated":
            case "customer.subscription.deleted":
                await HandleSubscriptionWebhookAsync(webhook, cancellationToken);
                break;
            case var _ when webhook.EventType.StartsWith("invoice.", StringComparison.OrdinalIgnoreCase):
                await HandleSubscriptionInvoiceWebhookAsync(webhook, cancellationToken);
                break;
            case var _ when webhook.EventType.StartsWith("charge.dispute.", StringComparison.OrdinalIgnoreCase):
                await HandleDisputeEventAsync(webhook, cancellationToken);
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

        if (invoiceStatus == SubscriptionInvoiceStatus.Paid)
            await EnsureSubscriptionAllocationRunAsync(invoice, subscription, plan, cancellationToken);
        else if (invoiceStatus is SubscriptionInvoiceStatus.Failed
                 or SubscriptionInvoiceStatus.Void
                 or SubscriptionInvoiceStatus.Uncollectible)
            await ReverseSubscriptionAllocationRunAsync(invoice.Id, cancellationToken);
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

    private async Task HandleRefundEventAsync(StripeWebhookEvent webhook, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(webhook.ProviderRefundId)
            || string.IsNullOrWhiteSpace(webhook.PaymentIntentId)
            || !webhook.AmountMinor.HasValue)
        {
            return;
        }

        var attempt = await _context.PaymentAttempts
            .FirstOrDefaultAsync(x => x.ProviderPaymentIntentId == webhook.PaymentIntentId, cancellationToken);
        if (attempt == null)
            return;

        var purchase = await _context.CoursePurchases
            .FirstOrDefaultAsync(x => x.PaymentAttemptId == attempt.Id, cancellationToken);
        var settlement = await _context.TeacherSettlements
            .FirstOrDefaultAsync(x => x.PaymentAttemptId == attempt.Id, cancellationToken);
        var adminId = webhook.Metadata.TryGetValue("adminId", out var requestedByAdminId)
            ? requestedByAdminId
            : null;

        await UpsertRefundRecordAsync(
            attempt,
            purchase,
            settlement,
            webhook.ProviderRefundId,
            webhook.PaymentIntentId,
            FromMinorUnits(webhook.AmountMinor.Value),
            webhook.Currency ?? attempt.Currency,
            ResolveRefundStatus(webhook),
            webhook.RefundReason,
            webhook.FailureMessage,
            adminId,
            cancellationToken);
    }

    private async Task HandleDisputeEventAsync(StripeWebhookEvent webhook, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(webhook.ProviderDisputeId)
            || string.IsNullOrWhiteSpace(webhook.PaymentIntentId)
            || !webhook.AmountMinor.HasValue)
        {
            return;
        }

        var attempt = await _context.PaymentAttempts
            .FirstOrDefaultAsync(x => x.ProviderPaymentIntentId == webhook.PaymentIntentId, cancellationToken);
        if (attempt == null)
            return;

        var purchase = await _context.CoursePurchases
            .FirstOrDefaultAsync(x => x.PaymentAttemptId == attempt.Id, cancellationToken);
        var settlement = await _context.TeacherSettlements
            .FirstOrDefaultAsync(x => x.PaymentAttemptId == attempt.Id, cancellationToken);

        var dispute = await UpsertDisputeRecordAsync(
            attempt,
            purchase,
            settlement,
            webhook.ProviderDisputeId,
            webhook.PaymentIntentId,
            FromMinorUnits(webhook.AmountMinor.Value),
            webhook.Currency ?? attempt.Currency,
            ResolveDisputeStatus(webhook),
            webhook.DisputeReason,
            webhook.DisputeEvidenceDueBy,
            cancellationToken);

        if (ShouldApplyDisputeLedger(webhook, dispute))
            await ApplyDisputeToLedgerAsync(dispute, attempt, purchase, settlement, cancellationToken);

        if (ShouldRestoreDisputeLedger(webhook, dispute))
            await RestoreDisputeLedgerAsync(dispute, attempt, purchase, settlement, cancellationToken);

        await ReconcileAttemptAndPurchaseStatusAsync(attempt, purchase, cancellationToken);
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
        var allocationLines = await _context.SubscriptionAllocationLines
            .Where(x => x.PayoutRecordId == payoutRecord.Id)
            .ToListAsync(cancellationToken);

        foreach (var settlement in settlements)
            await RecalculateSettlementStatusAsync(settlement, cancellationToken);

        foreach (var allocationLine in allocationLines)
            allocationLine.PaidOutAt ??= payoutRecord.PaidAt;
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
        var allocationLines = await _context.SubscriptionAllocationLines
            .Where(x => x.PayoutRecordId == payoutRecord.Id)
            .ToListAsync(cancellationToken);

        foreach (var settlement in settlements)
            await RecalculateSettlementStatusAsync(settlement, cancellationToken);

        foreach (var allocationLine in allocationLines)
        {
            allocationLine.PayoutRecordId = null;
            allocationLine.PaidOutAt = null;
        }
    }

    private async Task ApplyRefundToLedgerAsync(
        RefundRecord refund,
        PaymentAttempt attempt,
        CoursePurchase? purchase,
        TeacherSettlement? settlement,
        CancellationToken cancellationToken)
    {
        if (settlement != null)
        {
            var remainingGross = GetRemainingGrossAmount(settlement);
            var refundableGross = Math.Min(refund.Amount, remainingGross);

            if (refundableGross > 0)
            {
                var remainingNet = GetRemainingNetAmount(settlement);
                var teacherNetRefund = CalculateProportionalNetAmount(remainingGross, remainingNet, refundableGross);

                settlement.RefundedGrossAmount += refundableGross;
                settlement.RefundedNetAmount += teacherNetRefund;
                refund.TeacherNetRefundAmount = teacherNetRefund;

                await AdjustPendingPayoutAsync(settlement, teacherNetRefund, cancellationToken);
                await RecalculateSettlementStatusAsync(settlement, cancellationToken);
            }
        }

        await ReconcileAttemptAndPurchaseStatusAsync(attempt, purchase, cancellationToken);
        refund.LedgerAppliedAt = DateTime.UtcNow;
    }

    private async Task ApplyDisputeToLedgerAsync(
        DisputeRecord dispute,
        PaymentAttempt attempt,
        CoursePurchase? purchase,
        TeacherSettlement? settlement,
        CancellationToken cancellationToken)
    {
        if (dispute.LedgerAppliedAt != null)
            return;

        if (settlement != null)
        {
            var remainingGross = GetRemainingGrossAmount(settlement);
            var disputableGross = Math.Min(dispute.Amount, remainingGross);
            if (disputableGross > 0)
            {
                var remainingNet = GetRemainingNetAmount(settlement);
                var teacherNetDispute = CalculateProportionalNetAmount(remainingGross, remainingNet, disputableGross);

                settlement.DisputedGrossAmount += disputableGross;
                settlement.DisputedNetAmount += teacherNetDispute;
                dispute.AppliedGrossAmount = disputableGross;
                dispute.TeacherNetDisputeAmount = teacherNetDispute;
                dispute.FundsWithdrawnAt ??= DateTime.UtcNow;

                await AdjustPendingPayoutAsync(settlement, teacherNetDispute, cancellationToken);
                await RecalculateSettlementStatusAsync(settlement, cancellationToken);

                if (dispute.Status == DisputeRecordStatus.Lost && GetRemainingNetAmount(settlement) <= 0 && purchase != null)
                {
                    var revokeResult = await _courseAccessRevocationService.RevokeAccessAsync(
                        purchase.CourseId,
                        purchase.StudentId,
                        cancellationToken);

                    if (revokeResult.IsFailure)
                        throw new InvalidOperationException(revokeResult.Error);
                }
            }
        }

        dispute.LedgerAppliedAt = DateTime.UtcNow;
    }

    private async Task RestoreDisputeLedgerAsync(
        DisputeRecord dispute,
        PaymentAttempt attempt,
        CoursePurchase? purchase,
        TeacherSettlement? settlement,
        CancellationToken cancellationToken)
    {
        if (dispute.LedgerAppliedAt == null || dispute.LedgerRestoredAt != null)
            return;

        if (settlement != null)
        {
            settlement.DisputedGrossAmount = Math.Max(0m, settlement.DisputedGrossAmount - dispute.AppliedGrossAmount);
            settlement.DisputedNetAmount = Math.Max(0m, settlement.DisputedNetAmount - dispute.TeacherNetDisputeAmount);
            dispute.FundsReinstatedAt ??= DateTime.UtcNow;

            await AdjustPendingPayoutAsync(settlement, -dispute.TeacherNetDisputeAmount, cancellationToken);
            await RecalculateSettlementStatusAsync(settlement, cancellationToken);
        }

        dispute.LedgerRestoredAt = DateTime.UtcNow;
        await ReconcileAttemptAndPurchaseStatusAsync(attempt, purchase, cancellationToken);
    }

    private async Task<RefundRecord> UpsertRefundRecordAsync(
        PaymentAttempt attempt,
        CoursePurchase? purchase,
        TeacherSettlement? settlement,
        string providerRefundId,
        string providerPaymentIntentId,
        decimal amount,
        string currency,
        RefundRecordStatus status,
        string? reason,
        string? failureMessage,
        string? requestedByAdminId,
        CancellationToken cancellationToken)
    {
        var refund = await _context.RefundRecords
            .FirstOrDefaultAsync(x => x.ProviderRefundId == providerRefundId, cancellationToken);

        if (refund == null)
        {
            refund = new RefundRecord
            {
                PaymentAttemptId = attempt.Id,
                StudentId = attempt.StudentId,
                TeacherId = attempt.TeacherId,
                CourseTitle = attempt.CourseTitle,
                Provider = _paymentsOptions.Provider,
                ProviderRefundId = providerRefundId,
                RequestedAt = DateTime.UtcNow,
            };
            _context.RefundRecords.Add(refund);
        }

        refund.CoursePurchaseId = purchase?.Id;
        refund.TeacherSettlementId = settlement?.Id;
        refund.PayoutRecordId = settlement?.PayoutRecordId;
        refund.RequestedByAdminId ??= requestedByAdminId;
        refund.ProviderPaymentIntentId = providerPaymentIntentId;
        refund.Amount = amount;
        refund.Currency = currency;
        refund.Reason = reason;
        refund.FailureMessage = failureMessage;
        refund.Status = status;
        refund.ProcessedAt = status == RefundRecordStatus.Pending ? null : DateTime.UtcNow;

        if (refund.Status == RefundRecordStatus.Succeeded && refund.LedgerAppliedAt == null)
        {
            await ApplyRefundToLedgerAsync(refund, attempt, purchase, settlement, cancellationToken);
        }

        return refund;
    }

    private async Task<DisputeRecord> UpsertDisputeRecordAsync(
        PaymentAttempt attempt,
        CoursePurchase? purchase,
        TeacherSettlement? settlement,
        string providerDisputeId,
        string providerPaymentIntentId,
        decimal amount,
        string currency,
        DisputeRecordStatus status,
        string? reason,
        DateTime? evidenceDueBy,
        CancellationToken cancellationToken)
    {
        var dispute = await _context.DisputeRecords
            .FirstOrDefaultAsync(x => x.ProviderDisputeId == providerDisputeId, cancellationToken);

        if (dispute == null)
        {
            dispute = new DisputeRecord
            {
                PaymentAttemptId = attempt.Id,
                StudentId = attempt.StudentId,
                TeacherId = attempt.TeacherId,
                CourseTitle = attempt.CourseTitle,
                Provider = _paymentsOptions.Provider,
                ProviderDisputeId = providerDisputeId,
                OpenedAt = DateTime.UtcNow,
            };
            _context.DisputeRecords.Add(dispute);
        }

        dispute.CoursePurchaseId = purchase?.Id;
        dispute.TeacherSettlementId = settlement?.Id;
        dispute.PayoutRecordId = settlement?.PayoutRecordId;
        dispute.ProviderPaymentIntentId = providerPaymentIntentId;
        dispute.Amount = amount;
        dispute.Currency = currency;
        dispute.Status = status;
        dispute.Reason = reason;
        dispute.EvidenceDueBy = evidenceDueBy;

        if (status is DisputeRecordStatus.Won or DisputeRecordStatus.Lost or DisputeRecordStatus.WarningClosed or DisputeRecordStatus.Prevented)
            dispute.ClosedAt ??= DateTime.UtcNow;

        return dispute;
    }

    private async Task AdjustPendingPayoutAsync(
        TeacherSettlement settlement,
        decimal netAmountDelta,
        CancellationToken cancellationToken)
    {
        if (settlement.PayoutRecordId == null || netAmountDelta == 0)
            return;

        await RefreshPendingPayoutRecordAsync(settlement.PayoutRecordId.Value, cancellationToken);
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

    private async Task ReconcileAttemptAndPurchaseStatusAsync(
        PaymentAttempt attempt,
        CoursePurchase? purchase,
        CancellationToken cancellationToken)
    {
        var totalRefunded = await _context.RefundRecords
            .Where(x => x.PaymentAttemptId == attempt.Id && x.Status == RefundRecordStatus.Succeeded)
            .SumAsync(x => x.Amount, cancellationToken);
        var disputeStatuses = await _context.DisputeRecords
            .Where(x => x.PaymentAttemptId == attempt.Id)
            .Select(x => x.Status)
            .ToListAsync(cancellationToken);

        var isFullyRefunded = totalRefunded >= attempt.Amount;
        var hasOpenDispute = disputeStatuses.Any(IsOpenDisputeStatus);

        if (hasOpenDispute)
        {
            attempt.Status = PaymentAttemptStatus.Disputed;
            attempt.FailureMessage = "По платежу открыт спор / chargeback.";
        }
        else if (isFullyRefunded)
        {
            attempt.Status = PaymentAttemptStatus.Refunded;
            attempt.FailureMessage = "Оплата полностью возвращена.";
        }
        else if (totalRefunded > 0)
        {
            attempt.Status = PaymentAttemptStatus.PartiallyRefunded;
            attempt.FailureMessage = "Оплата частично возвращена.";
        }
        else if (attempt.CompletedAt.HasValue)
        {
            attempt.Status = PaymentAttemptStatus.Succeeded;
            attempt.FailureMessage = null;
        }

        if (purchase != null)
        {
            purchase.Status = hasOpenDispute
                ? CoursePurchaseStatus.Disputed
                : isFullyRefunded
                    ? CoursePurchaseStatus.Refunded
                    : totalRefunded > 0
                        ? CoursePurchaseStatus.PartiallyRefunded
                        : CoursePurchaseStatus.Active;
            await SyncCourseAccessForPurchaseAsync(attempt, purchase, cancellationToken);
        }
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

    private async Task SyncCourseAccessForPurchaseAsync(
        PaymentAttempt attempt,
        CoursePurchase purchase,
        CancellationToken cancellationToken)
    {
        if (purchase.Status is CoursePurchaseStatus.Active or CoursePurchaseStatus.PartiallyRefunded)
        {
            var grantResult = await _courseAccessProvisioningService.GrantAccessAsync(
                purchase.CourseId,
                purchase.StudentId,
                attempt.StudentName,
                cancellationToken);

            if (grantResult.IsFailure)
                throw new InvalidOperationException(grantResult.Error);

            return;
        }

        if (purchase.Status is CoursePurchaseStatus.Refunded or CoursePurchaseStatus.Revoked)
        {
            var revokeResult = await _courseAccessRevocationService.RevokeAccessAsync(
                purchase.CourseId,
                purchase.StudentId,
                cancellationToken);

            if (revokeResult.IsFailure)
                throw new InvalidOperationException(revokeResult.Error);
        }
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

    private async Task EnsureSubscriptionAllocationRunAsync(
        SubscriptionInvoice invoice,
        UserSubscription subscription,
        SubscriptionPlan plan,
        CancellationToken cancellationToken)
    {
        if (invoice.AmountPaid <= 0)
            return;

        var existingRun = await _context.SubscriptionAllocationRuns
            .AnyAsync(x => x.SubscriptionInvoiceId == invoice.Id, cancellationToken);
        if (existingRun)
            return;

        var candidates = await _subscriptionAllocationReadService.GetAllocationCandidatesAsync(
            invoice.UserId,
            invoice.PeriodStart,
            invoice.PeriodEnd,
            cancellationToken);

        var grossAmount = invoice.AmountPaid;
        var providerFeeAmount = 0m;
        var platformCommissionAmount = CalculatePlatformCommissionAmount(grossAmount);
        var netAmount = Math.Max(0m, grossAmount - providerFeeAmount - platformCommissionAmount);

        var run = new SubscriptionAllocationRun
        {
            SubscriptionInvoiceId = invoice.Id,
            UserSubscriptionId = invoice.UserSubscriptionId ?? subscription.Id,
            SubscriptionPlanId = plan.Id,
            UserId = invoice.UserId,
            PlanName = plan.Name,
            GrossAmount = grossAmount,
            PlatformCommissionAmount = platformCommissionAmount,
            ProviderFeeAmount = providerFeeAmount,
            NetAmount = netAmount,
            Currency = invoice.Currency,
            Strategy = "ProgressWeightedActiveEnrollmentsV1",
            Status = candidates.Count == 0
                ? SubscriptionAllocationRunStatus.Skipped
                : SubscriptionAllocationRunStatus.Applied,
            TeacherCount = candidates.Select(x => x.TeacherId).Distinct(StringComparer.Ordinal).Count(),
            CourseCount = candidates.Count,
            PeriodStart = invoice.PeriodStart,
            PeriodEnd = invoice.PeriodEnd,
            AllocatedAt = invoice.PaidAt ?? DateTime.UtcNow,
        };

        _context.SubscriptionAllocationRuns.Add(run);

        if (candidates.Count == 0 || netAmount <= 0)
            return;

        var weights = BuildSubscriptionAllocationWeights(candidates);
        var distributedGross = DistributeAmountByWeights(grossAmount, weights);
        var distributedPlatformCommission = DistributeAmountByWeights(platformCommissionAmount, weights);
        var distributedProviderFee = DistributeAmountByWeights(providerFeeAmount, weights);

        for (var i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            var lineGrossAmount = distributedGross[i];
            var linePlatformCommissionAmount = distributedPlatformCommission[i];
            var lineProviderFeeAmount = distributedProviderFee[i];
            var lineNetAmount = Math.Max(
                0m,
                lineGrossAmount - linePlatformCommissionAmount - lineProviderFeeAmount);

            _context.SubscriptionAllocationLines.Add(new SubscriptionAllocationLine
            {
                SubscriptionAllocationRunId = run.Id,
                SubscriptionInvoiceId = invoice.Id,
                SubscriptionPlanId = plan.Id,
                UserId = invoice.UserId,
                TeacherId = candidate.TeacherId,
                TeacherName = candidate.TeacherName,
                CourseId = candidate.CourseId,
                CourseTitle = candidate.CourseTitle,
                AllocationWeight = weights[i],
                ProgressPercent = candidate.ProgressPercent,
                TotalLessons = candidate.TotalLessons,
                CompletedLessons = candidate.CompletedLessons,
                GrossAmount = lineGrossAmount,
                PlatformCommissionAmount = linePlatformCommissionAmount,
                ProviderFeeAmount = lineProviderFeeAmount,
                NetAmount = lineNetAmount,
                Currency = invoice.Currency,
                AvailableAt = run.AllocatedAt.AddDays(Math.Max(0, _paymentsOptions.SettlementHoldDays)),
                AllocatedAt = run.AllocatedAt,
            });
        }
    }

    private async Task ReverseSubscriptionAllocationRunAsync(
        Guid subscriptionInvoiceId,
        CancellationToken cancellationToken)
    {
        var run = await _context.SubscriptionAllocationRuns
            .FirstOrDefaultAsync(x => x.SubscriptionInvoiceId == subscriptionInvoiceId, cancellationToken);
        if (run == null || run.Status != SubscriptionAllocationRunStatus.Applied)
            return;

        run.Status = SubscriptionAllocationRunStatus.Reversed;

        var lines = await _context.SubscriptionAllocationLines
            .Where(x => x.SubscriptionAllocationRunId == run.Id)
            .ToListAsync(cancellationToken);
        if (lines.Count == 0)
            return;

        var payoutIds = lines
            .Where(x => x.PayoutRecordId.HasValue)
            .Select(x => x.PayoutRecordId!.Value)
            .Distinct()
            .ToList();
        var payoutIdsToRefresh = new HashSet<Guid>();

        var payouts = payoutIds.Count == 0
            ? new Dictionary<Guid, PayoutRecord>()
            : await _context.PayoutRecords
                .Where(x => payoutIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var line in lines)
        {
            if (!line.PayoutRecordId.HasValue)
                continue;

            if (!payouts.TryGetValue(line.PayoutRecordId.Value, out var payoutRecord))
            {
                line.PayoutRecordId = null;
                line.PaidOutAt = null;
                continue;
            }

            if (payoutRecord.Status is PayoutRecordStatus.Queued
                or PayoutRecordStatus.SubmittedToProvider
                or PayoutRecordStatus.Canceled
                or PayoutRecordStatus.Failed
                or PayoutRecordStatus.Reversed)
            {
                if (payoutRecord.Status is PayoutRecordStatus.Queued or PayoutRecordStatus.SubmittedToProvider)
                    payoutIdsToRefresh.Add(payoutRecord.Id);

                line.PayoutRecordId = null;
                line.PaidOutAt = null;
            }
        }

        foreach (var payoutId in payoutIdsToRefresh)
            await RefreshPendingPayoutRecordAsync(payoutId, cancellationToken);
    }

    private static decimal[] BuildSubscriptionAllocationWeights(
        IReadOnlyList<SubscriptionAllocationCandidate> candidates)
    {
        if (candidates.Count == 0)
            return [];

        var progressWeightSum = candidates.Sum(x => x.ProgressPercent > 0 ? x.ProgressPercent : 0m);
        if (progressWeightSum > 0)
        {
            return candidates
                .Select(x => Math.Round(x.ProgressPercent / progressWeightSum, 6, MidpointRounding.AwayFromZero))
                .ToArray();
        }

        var equalWeight = Math.Round(1m / candidates.Count, 6, MidpointRounding.AwayFromZero);
        var weights = Enumerable.Repeat(equalWeight, candidates.Count).ToArray();
        weights[^1] = Math.Max(
            0m,
            Math.Round(1m - weights.Take(candidates.Count - 1).Sum(), 6, MidpointRounding.AwayFromZero));
        return weights;
    }

    private static decimal[] DistributeAmountByWeights(decimal totalAmount, IReadOnlyList<decimal> weights)
    {
        if (weights.Count == 0)
            return [];

        var distributed = new decimal[weights.Count];
        var allocated = 0m;

        for (var i = 0; i < weights.Count; i++)
        {
            distributed[i] = i == weights.Count - 1
                ? Math.Max(0m, Math.Round(totalAmount - allocated, 2, MidpointRounding.AwayFromZero))
                : Math.Round(totalAmount * weights[i], 2, MidpointRounding.AwayFromZero);
            allocated += distributed[i];
        }

        return distributed;
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
        var availableAt = DateTime.UtcNow.AddDays(Math.Max(0, _paymentsOptions.SettlementHoldDays));
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

    private static RefundRecordStatus ResolveRefundStatus(StripeWebhookEvent webhook)
    {
        if (string.Equals(webhook.EventType, "refund.failed", StringComparison.OrdinalIgnoreCase))
            return RefundRecordStatus.Failed;

        return webhook.RefundStatus?.ToLowerInvariant() switch
        {
            "succeeded" => RefundRecordStatus.Succeeded,
            "failed" => RefundRecordStatus.Failed,
            "canceled" => RefundRecordStatus.Canceled,
            _ => RefundRecordStatus.Pending,
        };
    }

    private static RefundRecordStatus ResolveRefundStatus(string? providerStatus)
    {
        return providerStatus?.ToLowerInvariant() switch
        {
            "succeeded" => RefundRecordStatus.Succeeded,
            "failed" => RefundRecordStatus.Failed,
            "canceled" => RefundRecordStatus.Canceled,
            _ => RefundRecordStatus.Pending,
        };
    }

    private static DisputeRecordStatus ResolveDisputeStatus(StripeWebhookEvent webhook)
    {
        return ResolveDisputeStatus(webhook.DisputeStatus ?? webhook.EventType);
    }

    private static DisputeRecordStatus ResolveDisputeStatus(string? providerStatus)
    {
        return providerStatus?.ToLowerInvariant() switch
        {
            "needs_response" or "charge.dispute.created" => DisputeRecordStatus.NeedsResponse,
            "under_review" or "charge.dispute.updated" => DisputeRecordStatus.UnderReview,
            "won" or "charge.dispute.funds_reinstated" => DisputeRecordStatus.Won,
            "lost" or "charge.dispute.funds_withdrawn" => DisputeRecordStatus.Lost,
            "warning_needs_response" or "charge.dispute.warning_needs_response" => DisputeRecordStatus.WarningNeedsResponse,
            "warning_under_review" or "charge.dispute.warning_under_review" => DisputeRecordStatus.WarningUnderReview,
            "warning_closed" or "charge.dispute.warning_closed" => DisputeRecordStatus.WarningClosed,
            "prevented" or "charge.dispute.closed" => DisputeRecordStatus.Prevented,
            _ => DisputeRecordStatus.UnderReview,
        };
    }

    private static bool ShouldApplyDisputeLedger(StripeWebhookEvent webhook, DisputeRecord dispute)
    {
        return dispute.LedgerAppliedAt == null
            && (string.Equals(webhook.EventType, "charge.dispute.funds_withdrawn", StringComparison.OrdinalIgnoreCase)
                || dispute.Status == DisputeRecordStatus.Lost);
    }

    private static bool ShouldRestoreDisputeLedger(StripeWebhookEvent webhook, DisputeRecord dispute)
    {
        return dispute.LedgerAppliedAt != null
            && dispute.LedgerRestoredAt == null
            && (string.Equals(webhook.EventType, "charge.dispute.funds_reinstated", StringComparison.OrdinalIgnoreCase)
                || dispute.Status is DisputeRecordStatus.Won or DisputeRecordStatus.WarningClosed or DisputeRecordStatus.Prevented);
    }

    private static bool IsOpenDisputeStatus(DisputeRecordStatus status)
    {
        return status is DisputeRecordStatus.NeedsResponse
            or DisputeRecordStatus.UnderReview
            or DisputeRecordStatus.Lost
            or DisputeRecordStatus.WarningNeedsResponse
            or DisputeRecordStatus.WarningUnderReview;
    }

    private static decimal GetRemainingGrossAmount(TeacherSettlement settlement)
    {
        return Math.Max(0m, settlement.GrossAmount - settlement.RefundedGrossAmount - settlement.DisputedGrossAmount);
    }

    private static decimal GetRemainingNetAmount(TeacherSettlement settlement)
    {
        return Math.Max(0m, settlement.NetAmount - settlement.RefundedNetAmount - settlement.DisputedNetAmount);
    }

    private static decimal CalculateProportionalNetAmount(
        decimal remainingGross,
        decimal remainingNet,
        decimal appliedGross)
    {
        if (remainingGross <= 0 || remainingNet <= 0 || appliedGross <= 0)
            return 0m;

        if (appliedGross >= remainingGross)
            return remainingNet;

        var proportionalNet = Math.Round(
            remainingNet * (appliedGross / remainingGross),
            2,
            MidpointRounding.AwayFromZero);

        return Math.Min(remainingNet, proportionalNet);
    }

    private static decimal FromMinorUnits(long amountMinor)
    {
        return decimal.Round(amountMinor / 100m, 2, MidpointRounding.AwayFromZero);
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

    private async Task RefreshPendingPayoutRecordAsync(
        Guid payoutRecordId,
        CancellationToken cancellationToken)
    {
        var payoutRecord = await _context.PayoutRecords
            .FirstOrDefaultAsync(x => x.Id == payoutRecordId, cancellationToken);
        if (payoutRecord == null
            || payoutRecord.Status is not (PayoutRecordStatus.Queued or PayoutRecordStatus.SubmittedToProvider))
        {
            return;
        }

        var remainingSettlements = await _context.TeacherSettlements
            .Where(x => x.PayoutRecordId == payoutRecord.Id)
            .ToListAsync(cancellationToken);
        var remainingAllocationLines = await (from line in _context.SubscriptionAllocationLines
                                              join run in _context.SubscriptionAllocationRuns
                                                on line.SubscriptionAllocationRunId equals run.Id
                                              where line.PayoutRecordId == payoutRecord.Id
                                                 && run.Status == SubscriptionAllocationRunStatus.Applied
                                              select line)
            .ToListAsync(cancellationToken);

        payoutRecord.Amount = remainingSettlements.Sum(GetRemainingNetAmount)
            + remainingAllocationLines.Sum(x => x.NetAmount);
        payoutRecord.SettlementsCount = remainingSettlements.Count(x => GetRemainingNetAmount(x) > 0);
        payoutRecord.AllocationLinesCount = remainingAllocationLines.Count(x => x.NetAmount > 0);

        if (payoutRecord.Amount <= 0)
            payoutRecord.Status = PayoutRecordStatus.Canceled;
    }

    private static string ResolveSubscriptionAllocationPayoutStatus(
        SubscriptionAllocationRunStatus runStatus,
        DateTime availableAt,
        PayoutRecordStatus? payoutStatus,
        DateTime now)
    {
        if (payoutStatus == PayoutRecordStatus.Paid)
            return "PaidOut";

        if (payoutStatus is PayoutRecordStatus.Queued or PayoutRecordStatus.SubmittedToProvider)
            return "InPayout";

        if (runStatus == SubscriptionAllocationRunStatus.Skipped)
            return "Skipped";

        if (runStatus is SubscriptionAllocationRunStatus.Reversed or SubscriptionAllocationRunStatus.Canceled)
            return runStatus.ToString();

        return availableAt <= now ? "ReadyForPayout" : "PendingHold";
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
        string? providerPriceId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Название тарифа обязательно.");
        if (price <= 0)
            throw new InvalidOperationException("Стоимость тарифа должна быть больше нуля.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidOperationException("Валюта тарифа обязательна.");
        if (billingIntervalCount <= 0)
            throw new InvalidOperationException("Интервал тарифа должен быть больше нуля.");

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
            record.AllocationLinesCount,
            record.Status.ToString(),
            record.ProviderTransferId,
            record.RequestedAt,
            record.SubmittedAt,
            record.PaidAt,
            record.FailedAt,
            record.FailureMessage);
    }

    private static RefundRecordDto MapRefundRecord(RefundRecord record, Guid courseId)
    {
        return new RefundRecordDto(
            record.Id,
            courseId,
            record.CourseTitle,
            record.Amount,
            record.TeacherNetRefundAmount,
            record.Currency,
            record.Status.ToString(),
            record.Reason,
            record.FailureMessage,
            record.RequestedAt,
            record.ProcessedAt);
    }

    private static DisputeRecordDto MapDisputeRecord(DisputeRecord record, Guid courseId)
    {
        return new DisputeRecordDto(
            record.Id,
            courseId,
            record.CourseTitle,
            record.Amount,
            record.TeacherNetDisputeAmount,
            record.Currency,
            record.Status.ToString(),
            record.Reason,
            record.OpenedAt,
            record.EvidenceDueBy,
            record.FundsWithdrawnAt,
            record.FundsReinstatedAt,
            record.ClosedAt);
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
}

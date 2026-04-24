using Auth.Infrastructure.Persistence;
using Courses.Domain.Enums;
using Courses.Infrastructure.Persistence;
using EduPlatform.Host.Models.Admin;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Enums;
using Payments.Infrastructure.Persistence;

namespace EduPlatform.Host.Services;

public class AdminAnalyticsReadService
{
    private readonly AuthDbContext _authDb;
    private readonly CoursesDbContext _coursesDb;
    private readonly PaymentsDbContext _paymentsDb;

    public AdminAnalyticsReadService(
        AuthDbContext authDb,
        CoursesDbContext coursesDb,
        PaymentsDbContext paymentsDb)
    {
        _authDb = authDb;
        _coursesDb = coursesDb;
        _paymentsDb = paymentsDb;
    }

    public async Task<AdminAnalyticsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var from30Days = now.Date.AddDays(-29);
        var from14Days = now.Date.AddDays(-13);

        var users = await _authDb.Users
            .AsNoTracking()
            .Select(u => new { u.Id, u.CreatedAt })
            .ToListAsync(cancellationToken);

        var courses = await _coursesDb.Courses
            .AsNoTracking()
            .Select(c => new
            {
                c.Id,
                c.Title,
                DisciplineName = c.Discipline.Name,
                c.TeacherId,
                c.TeacherName,
                c.IsPublished,
                c.IsArchived,
                c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var enrollments = await _coursesDb.CourseEnrollments
            .AsNoTracking()
            .Select(e => new
            {
                e.CourseId,
                e.StudentId,
                e.Status,
                e.EnrolledAt
            })
            .ToListAsync(cancellationToken);

        var paymentAttempts = await _paymentsDb.PaymentAttempts
            .AsNoTracking()
            .Select(p => new
            {
                p.CourseId,
                p.TeacherId,
                p.Amount,
                p.Currency,
                p.Status,
                p.CreatedAt,
                p.CompletedAt
            })
            .ToListAsync(cancellationToken);

        var teacherSettlements = await _paymentsDb.TeacherSettlements
            .AsNoTracking()
            .Select(s => new
            {
                s.PlatformCommissionAmount,
                s.Currency,
                s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var allocationRuns = await _paymentsDb.SubscriptionAllocationRuns
            .AsNoTracking()
            .Select(r => new
            {
                r.PlatformCommissionAmount,
                r.Currency,
                r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var refundRecords = await _paymentsDb.RefundRecords
            .AsNoTracking()
            .Select(r => new
            {
                r.Status,
                r.RequestedAt,
                r.ProcessedAt
            })
            .ToListAsync(cancellationToken);

        var disputeRecords = await _paymentsDb.DisputeRecords
            .AsNoTracking()
            .Select(d => new
            {
                d.Status,
                d.OpenedAt
            })
            .ToListAsync(cancellationToken);

        var subscriptionPlans = await _paymentsDb.SubscriptionPlans
            .AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.Currency,
                p.BillingInterval,
                p.BillingIntervalCount
            })
            .ToListAsync(cancellationToken);

        var subscriptions = await _paymentsDb.UserSubscriptions
            .AsNoTracking()
            .Select(s => new
            {
                s.SubscriptionPlanId,
                s.Status,
                s.Price,
                s.Currency,
                s.CanceledAt,
                s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var invoices = await _paymentsDb.SubscriptionInvoices
            .AsNoTracking()
            .Select(i => new
            {
                i.AmountPaid,
                i.Currency,
                i.Status,
                i.PaidAt,
                i.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var successfulPaymentStatuses = new[]
        {
            PaymentAttemptStatus.Succeeded,
            PaymentAttemptStatus.PartiallyRefunded,
            PaymentAttemptStatus.Refunded,
            PaymentAttemptStatus.Disputed,
        };

        var revenuePaymentRows = paymentAttempts
            .Where(p =>
                p.CompletedAt.HasValue
                && p.CompletedAt.Value >= from30Days
                && successfulPaymentStatuses.Contains(p.Status))
            .ToList();

        var paidInvoiceRows = invoices
            .Where(i =>
                i.Status == SubscriptionInvoiceStatus.Paid
                && i.PaidAt.HasValue
                && i.PaidAt.Value >= from30Days)
            .ToList();

        var grossRevenue30Days = BuildMoneyBreakdown(
            revenuePaymentRows.Select(x => new MoneyRow(x.Currency, x.Amount))
                .Concat(paidInvoiceRows.Select(x => new MoneyRow(x.Currency, x.AmountPaid))));

        var platformCommission30Days = BuildMoneyBreakdown(
            teacherSettlements
                .Where(x => x.CreatedAt >= from30Days)
                .Select(x => new MoneyRow(x.Currency, x.PlatformCommissionAmount))
                .Concat(allocationRuns
                    .Where(x => x.CreatedAt >= from30Days)
                    .Select(x => new MoneyRow(x.Currency, x.PlatformCommissionAmount))));

        var activeEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Active);
        var publishedCourses = courses.Count(c => c.IsPublished && !c.IsArchived);

        var successfulPayments30Days = revenuePaymentRows.Count;
        var failedPayments30Days = paymentAttempts.Count(p =>
            p.CreatedAt >= from30Days
            && (p.Status == PaymentAttemptStatus.Failed
                || p.Status == PaymentAttemptStatus.Canceled
                || p.Status == PaymentAttemptStatus.Expired));

        var refundedPayments30Days = refundRecords.Count(r =>
            r.Status == RefundRecordStatus.Succeeded
            && (r.ProcessedAt ?? r.RequestedAt) >= from30Days);

        var disputedPayments30Days = disputeRecords.Count(d =>
            d.OpenedAt >= from30Days);

        var activeSubscriptions = subscriptions.Count(s =>
            s.Status == UserSubscriptionStatus.Active
            || s.Status == UserSubscriptionStatus.Trialing
            || s.Status == UserSubscriptionStatus.PastDue);

        var plansById = subscriptionPlans.ToDictionary(
            plan => plan.Id,
            plan => new SubscriptionPlanSnapshot(
                plan.Currency,
                plan.BillingInterval,
                plan.BillingIntervalCount));

        var approximateMrrByCurrency = BuildMoneyBreakdown(
            subscriptions
                .Where(s =>
                    s.Status == UserSubscriptionStatus.Active
                    || s.Status == UserSubscriptionStatus.Trialing
                    || s.Status == UserSubscriptionStatus.PastDue)
                .Select(s =>
                {
                    var snapshot = plansById.TryGetValue(s.SubscriptionPlanId, out var plan)
                        ? plan
                        : new SubscriptionPlanSnapshot(s.Currency, SubscriptionBillingInterval.Month, 1);

                    return new MoneyRow(
                        snapshot.Currency,
                        NormalizeToMonthlyAmount(s.Price, snapshot.BillingInterval, snapshot.BillingIntervalCount));
                }));

        var trends = BuildTrends(
            from14Days,
            now.Date,
            users.Select(u => u.CreatedAt),
            enrollments.Select(e => e.EnrolledAt),
            revenuePaymentRows
                .Where(x => x.CompletedAt.HasValue && x.CompletedAt.Value >= from14Days)
                .Select(x => new RevenuePoint(x.CompletedAt!.Value, x.Currency, x.Amount))
                .Concat(paidInvoiceRows
                    .Where(x => x.PaidAt.HasValue && x.PaidAt.Value >= from14Days)
                    .Select(x => new RevenuePoint(x.PaidAt!.Value, x.Currency, x.AmountPaid)))
                .ToList());

        var activeStudentsByCourse = enrollments
            .Where(e => e.Status == EnrollmentStatus.Active)
            .GroupBy(e => e.CourseId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.StudentId).Distinct().Count());

        var revenueByCourse = paymentAttempts
            .Where(p =>
                successfulPaymentStatuses.Contains(p.Status))
            .GroupBy(p => new { p.CourseId, Currency = NormalizeCurrency(p.Currency) })
            .Select(g => new
            {
                g.Key.CourseId,
                g.Key.Currency,
                GrossRevenue = g.Sum(x => x.Amount)
            })
            .ToList();

        var topCourses = revenueByCourse
            .Join(
                courses,
                revenue => revenue.CourseId,
                course => course.Id,
                (revenue, course) => new AdminAnalyticsTopCourseDto
                {
                    CourseId = course.Id,
                    Title = course.Title,
                    DisciplineName = course.DisciplineName,
                    TeacherName = course.TeacherName,
                    IsPublished = course.IsPublished && !course.IsArchived,
                    ActiveStudents = activeStudentsByCourse.TryGetValue(course.Id, out var studentsCount) ? studentsCount : 0,
                    GrossRevenue = revenue.GrossRevenue,
                    Currency = revenue.Currency
                })
            .OrderByDescending(c => c.GrossRevenue)
            .ThenByDescending(c => c.ActiveStudents)
            .ThenBy(c => c.Title)
            .Take(6)
            .ToList();

        var publishedCoursesByTeacher = courses
            .Where(c => c.IsPublished && !c.IsArchived)
            .GroupBy(c => c.TeacherId)
            .ToDictionary(g => g.Key, g => g.Count());

        var activeStudentsByTeacher = courses
            .GroupJoin(
                enrollments.Where(e => e.Status == EnrollmentStatus.Active),
                course => course.Id,
                enrollment => enrollment.CourseId,
                (course, teacherEnrollments) => new
                {
                    course.TeacherId,
                    course.TeacherName,
                    Students = teacherEnrollments.Select(x => x.StudentId)
                })
            .GroupBy(x => new { x.TeacherId, x.TeacherName })
            .ToDictionary(
                g => (g.Key.TeacherId, g.Key.TeacherName),
                g => g.SelectMany(x => x.Students).Distinct().Count());

        var revenueByTeacher = paymentAttempts
            .Where(p =>
                successfulPaymentStatuses.Contains(p.Status))
            .GroupBy(p => new { p.TeacherId, Currency = NormalizeCurrency(p.Currency) })
            .Select(g => new
            {
                g.Key.TeacherId,
                g.Key.Currency,
                GrossRevenue = g.Sum(x => x.Amount)
            })
            .ToList();

        var topTeachers = courses
            .GroupBy(c => new { c.TeacherId, c.TeacherName })
            .SelectMany(g =>
            {
                var teacherRevenueRows = revenueByTeacher
                    .Where(r => r.TeacherId == g.Key.TeacherId)
                    .DefaultIfEmpty(new
                    {
                        TeacherId = g.Key.TeacherId,
                        Currency = "usd",
                        GrossRevenue = 0m
                    });

                return teacherRevenueRows.Select(revenue => new AdminAnalyticsTopTeacherDto
                {
                    TeacherId = g.Key.TeacherId,
                    TeacherName = g.Key.TeacherName,
                    PublishedCourses = publishedCoursesByTeacher.TryGetValue(g.Key.TeacherId, out var publishedCount) ? publishedCount : 0,
                    ActiveStudents = activeStudentsByTeacher.TryGetValue((g.Key.TeacherId, g.Key.TeacherName), out var studentsCount) ? studentsCount : 0,
                    GrossRevenue = revenue.GrossRevenue,
                    Currency = revenue.Currency
                });
            })
            .OrderByDescending(t => t.GrossRevenue)
            .ThenByDescending(t => t.ActiveStudents)
            .ThenByDescending(t => t.PublishedCourses)
            .ThenBy(t => t.TeacherName)
            .Take(6)
            .ToList();

        return new AdminAnalyticsDto
        {
            Summary = new AdminAnalyticsSummaryDto
            {
                TotalUsers = users.Count,
                NewUsers30Days = users.Count(u => u.CreatedAt >= from30Days),
                PublishedCourses = publishedCourses,
                ActiveEnrollments = activeEnrollments,
                GrossRevenue30DaysByCurrency = grossRevenue30Days,
                PlatformCommission30DaysByCurrency = platformCommission30Days,
                ActiveSubscriptions = activeSubscriptions,
                PaidInvoices30Days = paidInvoiceRows.Count
            },
            Payments = new AdminAnalyticsPaymentsDto
            {
                SuccessfulPayments30Days = successfulPayments30Days,
                FailedPayments30Days = failedPayments30Days,
                RefundedPayments30Days = refundedPayments30Days,
                DisputedPayments30Days = disputedPayments30Days,
                CoursePurchases30Days = revenuePaymentRows.Count,
                SubscriptionInvoicesPaid30Days = paidInvoiceRows.Count
            },
            Subscriptions = new AdminAnalyticsSubscriptionsDto
            {
                ActiveCount = subscriptions.Count(s => s.Status == UserSubscriptionStatus.Active),
                TrialingCount = subscriptions.Count(s => s.Status == UserSubscriptionStatus.Trialing),
                PastDueCount = subscriptions.Count(s => s.Status == UserSubscriptionStatus.PastDue),
                Canceled30Days = subscriptions.Count(s => s.CanceledAt.HasValue && s.CanceledAt.Value >= from30Days),
                ApproximateMonthlyRecurringRevenueByCurrency = approximateMrrByCurrency
            },
            Trends = trends,
            TopCourses = topCourses,
            TopTeachers = topTeachers
        };
    }

    private static List<AdminAnalyticsTrendPointDto> BuildTrends(
        DateTime from,
        DateTime to,
        IEnumerable<DateTime> userDates,
        IEnumerable<DateTime> enrollmentDates,
        IEnumerable<RevenuePoint> revenuePoints)
    {
        var userBuckets = userDates
            .Where(date => date >= from && date <= to.AddDays(1))
            .GroupBy(date => date.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var enrollmentBuckets = enrollmentDates
            .Where(date => date >= from && date <= to.AddDays(1))
            .GroupBy(date => date.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var revenueBuckets = revenuePoints
            .Where(point => point.Date >= from && point.Date <= to.AddDays(1))
            .GroupBy(point => point.Date.Date)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new MoneyRow(x.Currency, x.Amount)).ToList());

        var items = new List<AdminAnalyticsTrendPointDto>();
        for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
        {
            items.Add(new AdminAnalyticsTrendPointDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                Label = date.ToString("dd.MM"),
                NewUsers = userBuckets.TryGetValue(date, out var newUsers) ? newUsers : 0,
                NewEnrollments = enrollmentBuckets.TryGetValue(date, out var newEnrollments) ? newEnrollments : 0,
                RevenueByCurrency = revenueBuckets.TryGetValue(date, out var revenue)
                    ? BuildMoneyBreakdown(revenue)
                    : []
            });
        }

        return items;
    }

    private static List<AdminMoneyAmountDto> BuildMoneyBreakdown(IEnumerable<MoneyRow> rows)
    {
        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.Currency))
            .GroupBy(row => NormalizeCurrency(row.Currency))
            .Select(g => new AdminMoneyAmountDto
            {
                Currency = g.Key,
                Amount = g.Sum(x => x.Amount)
            })
            .Where(x => x.Amount != 0m)
            .OrderBy(x => x.Currency)
            .ToList();
    }

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? "usd"
            : currency.Trim().ToLowerInvariant();
    }

    private static decimal NormalizeToMonthlyAmount(
        decimal price,
        SubscriptionBillingInterval billingInterval,
        int billingIntervalCount)
    {
        var normalizedCount = Math.Max(billingIntervalCount, 1);
        var monthFactor = billingInterval == SubscriptionBillingInterval.Year
            ? 12m * normalizedCount
            : normalizedCount;

        return monthFactor > 0 ? price / monthFactor : price;
    }

    private sealed record RevenuePoint(DateTime Date, string Currency, decimal Amount);
    private sealed record MoneyRow(string Currency, decimal Amount);
    private sealed record SubscriptionPlanSnapshot(
        string Currency,
        SubscriptionBillingInterval BillingInterval,
        int BillingIntervalCount);
}

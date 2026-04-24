namespace EduPlatform.Host.Models.Admin;

public class AdminAnalyticsDto
{
    public AdminAnalyticsSummaryDto Summary { get; set; } = new();
    public AdminAnalyticsPaymentsDto Payments { get; set; } = new();
    public AdminAnalyticsSubscriptionsDto Subscriptions { get; set; } = new();
    public List<AdminAnalyticsTrendPointDto> Trends { get; set; } = [];
    public List<AdminAnalyticsTopCourseDto> TopCourses { get; set; } = [];
    public List<AdminAnalyticsTopTeacherDto> TopTeachers { get; set; } = [];
}

public class AdminAnalyticsSummaryDto
{
    public int TotalUsers { get; set; }
    public int NewUsers30Days { get; set; }
    public int PublishedCourses { get; set; }
    public int ActiveEnrollments { get; set; }
    public List<AdminMoneyAmountDto> GrossRevenue30DaysByCurrency { get; set; } = [];
    public List<AdminMoneyAmountDto> PlatformCommission30DaysByCurrency { get; set; } = [];
    public int ActiveSubscriptions { get; set; }
    public int PaidInvoices30Days { get; set; }
}

public class AdminAnalyticsPaymentsDto
{
    public int SuccessfulPayments30Days { get; set; }
    public int FailedPayments30Days { get; set; }
    public int RefundedPayments30Days { get; set; }
    public int DisputedPayments30Days { get; set; }
    public int CoursePurchases30Days { get; set; }
    public int SubscriptionInvoicesPaid30Days { get; set; }
}

public class AdminAnalyticsSubscriptionsDto
{
    public int ActiveCount { get; set; }
    public int TrialingCount { get; set; }
    public int PastDueCount { get; set; }
    public int Canceled30Days { get; set; }
    public List<AdminMoneyAmountDto> ApproximateMonthlyRecurringRevenueByCurrency { get; set; } = [];
}

public class AdminAnalyticsTrendPointDto
{
    public string Date { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int NewUsers { get; set; }
    public int NewEnrollments { get; set; }
    public List<AdminMoneyAmountDto> RevenueByCurrency { get; set; } = [];
}

public class AdminMoneyAmountDto
{
    public string Currency { get; set; } = "usd";
    public decimal Amount { get; set; }
}

public class AdminAnalyticsTopCourseDto
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DisciplineName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public int ActiveStudents { get; set; }
    public decimal GrossRevenue { get; set; }
    public string Currency { get; set; } = "usd";
}

public class AdminAnalyticsTopTeacherDto
{
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public int PublishedCourses { get; set; }
    public int ActiveStudents { get; set; }
    public decimal GrossRevenue { get; set; }
    public string Currency { get; set; } = "usd";
}

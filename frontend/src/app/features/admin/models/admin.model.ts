export interface AdminUserDto {
  id: string;
  email: string;
  fullName: string;
  role: string;
  isBlocked: boolean;
  emailConfirmed: boolean;
  createdAt: string;
}

export interface AdminCourseDto {
  id: string;
  title: string;
  teacherId: string;
  teacherName: string;
  disciplineId: string;
  disciplineName: string;
  isPublished: boolean;
  isArchived: boolean;
  archiveReason?: string;
  studentsCount: number;
  modulesCount: number;
  createdAt: string;
}

export interface PlatformSettingsDto {
  registrationOpen: boolean;
  maintenanceMode: boolean;
  platformName: string;
  supportEmail: string;
}

export interface UserStatsDto {
  total: number;
  students: number;
  teachers: number;
  admins: number;
  blocked: number;
  unconfirmedEmail: number;
  newLast7Days: number;
}

export interface CourseStatsDto {
  total: number;
  published: number;
  drafts: number;
  archived: number;
  totalEnrollments: number;
  disciplines: number;
}

export interface DashboardStatsDto {
  users: UserStatsDto;
  courses: CourseStatsDto;
}

export interface AdminMoneyAmountDto {
  currency: string;
  amount: number;
}

export interface AdminAnalyticsSummaryDto {
  totalUsers: number;
  newUsers30Days: number;
  publishedCourses: number;
  activeEnrollments: number;
  grossRevenue30DaysByCurrency: AdminMoneyAmountDto[];
  platformCommission30DaysByCurrency: AdminMoneyAmountDto[];
  activeSubscriptions: number;
  paidInvoices30Days: number;
}

export interface AdminAnalyticsPaymentsDto {
  successfulPayments30Days: number;
  failedPayments30Days: number;
  refundedPayments30Days: number;
  disputedPayments30Days: number;
  coursePurchases30Days: number;
  subscriptionInvoicesPaid30Days: number;
}

export interface AdminAnalyticsSubscriptionsDto {
  activeCount: number;
  trialingCount: number;
  pastDueCount: number;
  canceled30Days: number;
  approximateMonthlyRecurringRevenueByCurrency: AdminMoneyAmountDto[];
}

export interface AdminAnalyticsTrendPointDto {
  date: string;
  label: string;
  newUsers: number;
  newEnrollments: number;
  revenueByCurrency: AdminMoneyAmountDto[];
}

export interface AdminAnalyticsTopCourseDto {
  courseId: string;
  title: string;
  disciplineName: string;
  teacherName: string;
  isPublished: boolean;
  activeStudents: number;
  grossRevenue: number;
  currency: string;
}

export interface AdminAnalyticsTopTeacherDto {
  teacherId: string;
  teacherName: string;
  publishedCourses: number;
  activeStudents: number;
  grossRevenue: number;
  currency: string;
}

export interface AdminAnalyticsDto {
  summary: AdminAnalyticsSummaryDto;
  payments: AdminAnalyticsPaymentsDto;
  subscriptions: AdminAnalyticsSubscriptionsDto;
  trends: AdminAnalyticsTrendPointDto[];
  topCourses: AdminAnalyticsTopCourseDto[];
  topTeachers: AdminAnalyticsTopTeacherDto[];
}

export interface CreateUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  password: string;
}

export interface ChangeRoleRequest {
  role: string;
}

export interface ForceArchiveRequest {
  reason: string;
}

export interface AdminPaymentRecordDto {
  paymentAttemptId: string;
  courseId: string;
  courseTitle: string;
  studentId: string;
  studentName: string;
  teacherId: string;
  teacherName: string;
  amount: number;
  refundedAmount: number;
  pendingRefundAmount: number;
  disputedAmount: number;
  remainingRefundableAmount: number;
  providerFeeAmount: number;
  currency: string;
  paymentStatus: string;
  providerChargeId?: string | null;
  latestDisputeStatus?: string | null;
  purchaseStatus?: string | null;
  createdAt: string;
  completedAt?: string | null;
}

export interface AdminRefundRequest {
  paymentAttemptId: string;
  amount?: number | null;
  reason?: string | null;
}

export interface AdminSubscriptionPlanDto {
  id: string;
  name: string;
  description?: string | null;
  price: number;
  currency: string;
  billingInterval: string;
  billingIntervalCount: number;
  isActive: boolean;
  isFeatured: boolean;
  sortOrder: number;
  providerProductId?: string | null;
  providerPriceId?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface AdminSubscriptionAllocationLineDto {
  id: string;
  teacherId: string;
  teacherName: string;
  courseId: string;
  courseTitle: string;
  allocationWeight: number;
  progressPercent: number;
  completedLessons: number;
  totalLessons: number;
  grossAmount: number;
  platformCommissionAmount: number;
  providerFeeAmount: number;
  netAmount: number;
  currency: string;
}

export interface AdminSubscriptionAllocationRunDto {
  id: string;
  subscriptionInvoiceId: string;
  studentId: string;
  subscriptionPlanId: string;
  planName: string;
  grossAmount: number;
  platformCommissionAmount: number;
  providerFeeAmount: number;
  netAmount: number;
  currency: string;
  strategy: string;
  status: string;
  teacherCount: number;
  courseCount: number;
  periodStart?: string | null;
  periodEnd?: string | null;
  allocatedAt: string;
  lines: AdminSubscriptionAllocationLineDto[];
}

export interface UpsertSubscriptionPlanRequest {
  name: string;
  description?: string | null;
  price: number;
  currency: string;
  billingInterval: string;
  billingIntervalCount: number;
  isActive: boolean;
  isFeatured: boolean;
  sortOrder: number;
  providerProductId?: string | null;
  providerPriceId?: string | null;
}

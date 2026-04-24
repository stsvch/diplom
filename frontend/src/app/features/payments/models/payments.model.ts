export interface CourseCheckoutSessionDto {
  paymentAttemptId: string;
  checkoutUrl: string;
}

export interface SubscriptionCheckoutSessionDto {
  subscriptionPaymentAttemptId: string;
  checkoutUrl: string;
}

export interface SubscriptionPlanDto {
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

export interface UserSubscriptionDto {
  id: string;
  subscriptionPlanId: string;
  planName: string;
  price: number;
  currency: string;
  status: string;
  currentPeriodStart?: string | null;
  currentPeriodEnd?: string | null;
  cancelAtPeriodEnd: boolean;
  canceledAt?: string | null;
  startedAt: string;
  endedAt?: string | null;
}

export interface SubscriptionPaymentAttemptDto {
  id: string;
  subscriptionPlanId: string;
  planName: string;
  amount: number;
  currency: string;
  billingInterval: string;
  billingIntervalCount: number;
  status: string;
  failureMessage?: string | null;
  createdAt: string;
  completedAt?: string | null;
}

export interface SubscriptionInvoiceDto {
  id: string;
  subscriptionPlanId: string;
  planName: string;
  amountDue: number;
  amountPaid: number;
  currency: string;
  status: string;
  billingReason?: string | null;
  periodStart?: string | null;
  periodEnd?: string | null;
  dueDate?: string | null;
  paidAt?: string | null;
  failureMessage?: string | null;
  createdAt: string;
}

export interface PaymentAttemptDto {
  id: string;
  courseId: string;
  courseTitle: string;
  amount: number;
  currency: string;
  status: string;
  providerChargeId?: string | null;
  failureMessage?: string | null;
  createdAt: string;
  completedAt?: string | null;
}

export interface CoursePurchaseDto {
  id: string;
  courseId: string;
  courseTitle: string;
  amount: number;
  currency: string;
  status: string;
  purchasedAt: string;
}

export interface PaymentMethodRefDto {
  id: string;
  brand?: string | null;
  last4?: string | null;
  expMonth?: number | null;
  expYear?: number | null;
  isDefault: boolean;
}

export interface TeacherPayoutAccountDto {
  status: string;
  providerConfigured: boolean;
  chargesEnabled: boolean;
  payoutsEnabled: boolean;
  detailsSubmitted: boolean;
  canPublishPaidCourses: boolean;
  requirementsSummary?: string | null;
}

export interface TeacherSettlementSummaryDto {
  totalGrossAmount: number;
  totalNetAmount: number;
  pendingNetAmount: number;
  readyForPayoutNetAmount: number;
  inPayoutNetAmount: number;
  paidOutNetAmount: number;
  refundedNetAmount: number;
  disputedNetAmount: number;
  settlementsCount: number;
  subscriptionAllocationCount: number;
  currency: string;
}

export interface TeacherSettlementDto {
  id: string;
  courseId: string;
  courseTitle: string;
  studentName: string;
  grossAmount: number;
  providerFeeAmount: number;
  platformCommissionAmount: number;
  netAmount: number;
  refundedGrossAmount: number;
  refundedNetAmount: number;
  disputedGrossAmount: number;
  disputedNetAmount: number;
  currency: string;
  status: string;
  availableAt: string;
  paidOutAt?: string | null;
  createdAt: string;
}

export interface TeacherSubscriptionAllocationDto {
  id: string;
  subscriptionAllocationRunId: string;
  subscriptionInvoiceId: string;
  subscriptionPlanId: string;
  planName: string;
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
  status: string;
  payoutStatus: string;
  periodStart?: string | null;
  periodEnd?: string | null;
  availableAt: string;
  paidOutAt?: string | null;
  allocatedAt: string;
}

export interface PayoutRecordDto {
  id: string;
  amount: number;
  currency: string;
  settlementsCount: number;
  allocationLinesCount: number;
  status: string;
  providerTransferId?: string | null;
  requestedAt: string;
  submittedAt?: string | null;
  paidAt?: string | null;
  failedAt?: string | null;
  failureMessage?: string | null;
}

export interface RefundRecordDto {
  id: string;
  courseId: string;
  courseTitle: string;
  amount: number;
  teacherNetRefundAmount: number;
  currency: string;
  status: string;
  reason?: string | null;
  failureMessage?: string | null;
  requestedAt: string;
  processedAt?: string | null;
}

export interface DisputeRecordDto {
  id: string;
  courseId: string;
  courseTitle: string;
  amount: number;
  teacherNetDisputeAmount: number;
  currency: string;
  status: string;
  reason?: string | null;
  openedAt: string;
  evidenceDueBy?: string | null;
  fundsWithdrawnAt?: string | null;
  fundsReinstatedAt?: string | null;
  closedAt?: string | null;
}

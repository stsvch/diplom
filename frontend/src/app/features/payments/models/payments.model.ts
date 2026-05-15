// payments.model.ts
// Модели описывают DTO и типы, которыми frontend обменивается с backend API.
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
  individualSlotsPerMonth: number;
  groupSlotsPerMonth: number;
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
  individualSlotsPerMonth: number;
  groupSlotsPerMonth: number;
}

export interface UserEntitlementsDto {
  hasActiveSubscription: boolean;
  planName?: string | null;
  individualSlotsPerMonth: number;
  individualSlotsUsed: number;
  individualSlotsRemaining: number;
  groupSlotsPerMonth: number;
  groupSlotsUsed: number;
  groupSlotsRemaining: number;
  currentPeriodStart?: string | null;
  currentPeriodEnd?: string | null;
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
  settlementsCount: number;
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
  currency: string;
  status: string;
  availableAt: string;
  paidOutAt?: string | null;
  createdAt: string;
}

export interface PayoutRecordDto {
  id: string;
  amount: number;
  currency: string;
  settlementsCount: number;
  status: string;
  providerTransferId?: string | null;
  requestedAt: string;
  submittedAt?: string | null;
  paidAt?: string | null;
  failedAt?: string | null;
  failureMessage?: string | null;
}

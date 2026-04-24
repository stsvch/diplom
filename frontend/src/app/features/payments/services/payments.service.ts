import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CourseCheckoutSessionDto,
  CoursePurchaseDto,
  DisputeRecordDto,
  PaymentAttemptDto,
  PaymentMethodRefDto,
  PayoutRecordDto,
  RefundRecordDto,
  SubscriptionCheckoutSessionDto,
  SubscriptionInvoiceDto,
  SubscriptionPaymentAttemptDto,
  SubscriptionPlanDto,
  TeacherPayoutAccountDto,
  TeacherSettlementDto,
  TeacherSettlementSummaryDto,
  TeacherSubscriptionAllocationDto,
  UserSubscriptionDto,
} from '../models/payments.model';

@Injectable({
  providedIn: 'root',
})
export class PaymentsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/payments`;

  createCourseCheckout(courseId: string, savePaymentMethod = false): Observable<CourseCheckoutSessionDto> {
    return this.http.post<CourseCheckoutSessionDto>(`${this.base}/course-checkout`, {
      courseId,
      savePaymentMethod,
    });
  }

  createSubscriptionCheckout(subscriptionPlanId: string): Observable<SubscriptionCheckoutSessionDto> {
    return this.http.post<SubscriptionCheckoutSessionDto>(`${this.base}/subscription-checkout`, {
      subscriptionPlanId,
    });
  }

  getMyPaymentHistory(): Observable<PaymentAttemptDto[]> {
    return this.http.get<PaymentAttemptDto[]>(`${this.base}/me/history`);
  }

  getMySubscriptions(): Observable<UserSubscriptionDto[]> {
    return this.http.get<UserSubscriptionDto[]>(`${this.base}/me/subscriptions`);
  }

  getMySubscriptionHistory(): Observable<SubscriptionPaymentAttemptDto[]> {
    return this.http.get<SubscriptionPaymentAttemptDto[]>(`${this.base}/me/subscription-history`);
  }

  getMySubscriptionInvoices(): Observable<SubscriptionInvoiceDto[]> {
    return this.http.get<SubscriptionInvoiceDto[]>(`${this.base}/me/subscription-invoices`);
  }

  getMyPurchases(): Observable<CoursePurchaseDto[]> {
    return this.http.get<CoursePurchaseDto[]>(`${this.base}/me/purchases`);
  }

  getMyRefunds(): Observable<RefundRecordDto[]> {
    return this.http.get<RefundRecordDto[]>(`${this.base}/me/refunds`);
  }

  getMyDisputes(): Observable<DisputeRecordDto[]> {
    return this.http.get<DisputeRecordDto[]>(`${this.base}/me/disputes`);
  }

  getMyPaymentMethods(): Observable<PaymentMethodRefDto[]> {
    return this.http.get<PaymentMethodRefDto[]>(`${this.base}/me/payment-methods`);
  }

  getSubscriptionPlans(): Observable<SubscriptionPlanDto[]> {
    return this.http.get<SubscriptionPlanDto[]>(`${this.base}/subscription-plans`);
  }

  removePaymentMethod(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/me/payment-methods/${id}`);
  }

  getPaymentAttempt(id: string): Observable<PaymentAttemptDto> {
    return this.http.get<PaymentAttemptDto>(`${this.base}/attempts/${id}`);
  }

  cancelPaymentAttempt(id: string): Observable<PaymentAttemptDto> {
    return this.http.post<PaymentAttemptDto>(`${this.base}/attempts/${id}/cancel-return`, {});
  }

  getSubscriptionPaymentAttempt(id: string): Observable<SubscriptionPaymentAttemptDto> {
    return this.http.get<SubscriptionPaymentAttemptDto>(`${this.base}/subscription-attempts/${id}`);
  }

  getTeacherPayoutAccount(): Observable<TeacherPayoutAccountDto> {
    return this.http.get<TeacherPayoutAccountDto>(`${this.base}/teacher/payout-account`);
  }

  createTeacherOnboardingLink(): Observable<{ url: string }> {
    return this.http.post<{ url: string }>(`${this.base}/teacher/payout-account/onboarding-link`, {});
  }

  createTeacherDashboardLink(): Observable<{ url: string }> {
    return this.http.post<{ url: string }>(`${this.base}/teacher/payout-account/dashboard-link`, {});
  }

  getTeacherSettlementSummary(): Observable<TeacherSettlementSummaryDto> {
    return this.http.get<TeacherSettlementSummaryDto>(`${this.base}/teacher/summary`);
  }

  getTeacherSettlements(): Observable<TeacherSettlementDto[]> {
    return this.http.get<TeacherSettlementDto[]>(`${this.base}/teacher/settlements`);
  }

  getTeacherSubscriptionAllocations(): Observable<TeacherSubscriptionAllocationDto[]> {
    return this.http.get<TeacherSubscriptionAllocationDto[]>(`${this.base}/teacher/subscription-allocations`);
  }

  getTeacherPayoutRecords(): Observable<PayoutRecordDto[]> {
    return this.http.get<PayoutRecordDto[]>(`${this.base}/teacher/payouts`);
  }

  getTeacherDisputes(): Observable<DisputeRecordDto[]> {
    return this.http.get<DisputeRecordDto[]>(`${this.base}/teacher/disputes`);
  }

  requestTeacherPayout(): Observable<PayoutRecordDto> {
    return this.http.post<PayoutRecordDto>(`${this.base}/teacher/payouts/request`, {});
  }
}

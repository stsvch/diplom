// student-payments.component.ts
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { PaymentsService } from '../services/payments.service';
import {
  PaymentAttemptDto,
  SubscriptionInvoiceDto,
  SubscriptionPaymentAttemptDto,
  SubscriptionPlanDto,
  UserSubscriptionDto,
} from '../models/payments.model';
import { parseApiError } from '../../../core/models/api-error.model';

export interface HistoryItem {
  id: string;
  date: string;
  description: string;
  amount: number;
  currency: string;
  status: string;
  failureMessage?: string | null;
}

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-student-payments',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonComponent],
  templateUrl: './student-payments.component.html',
  styleUrl: './student-payments.component.scss',
})
export class StudentPaymentsComponent implements OnInit {
  private readonly paymentsService = inject(PaymentsService);
  private readonly route = inject(ActivatedRoute);

  // Signals и computed-значения хранят реактивное состояние без ручной синхронизации с шаблоном.
  readonly loadingAttempt = signal(false);
  readonly error = signal<string | null>(null);
  readonly currentAttemptState = signal<'success' | 'cancel' | null>(null);
  readonly currentAttempt = signal<PaymentAttemptDto | null>(null);
  readonly currentSubscriptionAttemptState = signal<'success' | 'cancel' | null>(null);
  readonly currentSubscriptionAttempt = signal<SubscriptionPaymentAttemptDto | null>(null);

  readonly courseHistory = signal<PaymentAttemptDto[]>([]);
  readonly subscriptions = signal<UserSubscriptionDto[]>([]);
  readonly subscriptionHistory = signal<SubscriptionPaymentAttemptDto[]>([]);
  readonly subscriptionInvoices = signal<SubscriptionInvoiceDto[]>([]);
  readonly subscriptionPlans = signal<SubscriptionPlanDto[]>([]);

  readonly activeSubscription = computed(() =>
    this.subscriptions().find((s) => s.status !== 'Canceled' && s.status !== 'Revoked') ?? null,
  );

  readonly offeredPlan = computed(() => {
    if (this.activeSubscription()) return null;
    return this.subscriptionPlans().find((p) => p.isActive) ?? null;
  });

  readonly history = computed<HistoryItem[]>(() => {
    const items: HistoryItem[] = [];
    const invoices = this.subscriptionInvoices();
    const hasInvoices = invoices.length > 0;

    for (const invoice of invoices) {
      items.push({
        id: `inv-${invoice.id}`,
        date: invoice.paidAt || invoice.createdAt,
        description: invoice.planName,
        amount: invoice.amountPaid || invoice.amountDue,
        currency: invoice.currency,
        status: invoice.status,
        failureMessage: invoice.failureMessage,
      });
    }

    for (const attempt of this.subscriptionHistory()) {
      if (hasInvoices && attempt.status === 'Succeeded') continue;
      items.push({
        id: `sub-${attempt.id}`,
        date: attempt.completedAt || attempt.createdAt,
        description: attempt.planName,
        amount: attempt.amount,
        currency: attempt.currency,
        status: attempt.status,
        failureMessage: attempt.failureMessage,
      });
    }

    for (const attempt of this.courseHistory()) {
      items.push({
        id: `att-${attempt.id}`,
        date: attempt.completedAt || attempt.createdAt,
        description: attempt.courseTitle,
        amount: attempt.amount,
        currency: attempt.currency,
        status: attempt.status,
        failureMessage: attempt.failureMessage,
      });
    }

    return items.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
  });

  // Lifecycle hook запускает первичную загрузку или очистку ресурсов компонента.
  ngOnInit(): void {
    this.loadPage();

    this.route.queryParamMap.subscribe((params) => {
      const attemptId = params.get('attempt');
      const state = params.get('state');
      const subscriptionAttemptId = params.get('subscriptionAttempt');
      const subscriptionState = params.get('subscriptionState');
      this.currentAttemptState.set(state === 'success' || state === 'cancel' ? state : null);
      this.currentSubscriptionAttemptState.set(
        subscriptionState === 'success' || subscriptionState === 'cancel' ? subscriptionState : null,
      );

      if (!attemptId) {
        this.currentAttempt.set(null);
      } else if (state === 'cancel') {
        this.loadingAttempt.set(true);
        // Подписка синхронизирует ответ сервиса с локальным состоянием и уведомлениями.
        this.paymentsService.cancelPaymentAttempt(attemptId).subscribe({
          next: (attempt) => {
            this.currentAttempt.set(attempt);
            this.loadingAttempt.set(false);
            this.loadCourseHistory();
          },
          error: () => this.loadCurrentAttempt(attemptId),
        });
      } else {
        this.loadCurrentAttempt(attemptId);
      }

      if (!subscriptionAttemptId) {
        this.currentSubscriptionAttempt.set(null);
      } else {
        this.loadingAttempt.set(true);
        this.paymentsService.getSubscriptionPaymentAttempt(subscriptionAttemptId).subscribe({
          next: (attempt) => {
            this.currentSubscriptionAttempt.set(attempt);
            this.loadingAttempt.set(false);
            this.loadSubscriptions();
            this.loadSubscriptionHistory();
            this.loadSubscriptionInvoices();
          },
          error: (err) => {
            this.error.set(parseApiError(err).message);
            this.loadingAttempt.set(false);
          },
        });
      }
    });
  }

  formatAmount(amount: number, currency: string): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: currency.toUpperCase(),
      maximumFractionDigits: 2,
    }).format(amount);
  }

  formatDate(value: string): string {
    return new Date(value).toLocaleDateString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  }

  getStatusLabel(status: string): string {
    const map: Record<string, string> = {
      Pending: 'Ожидает',
      Initiated: 'Создана',
      PendingProvider: 'Ожидает оплату',
      Succeeded: 'Оплачено',
      Failed: 'Ошибка',
      Canceled: 'Отменено',
      Expired: 'Истекло',
      Open: 'Открыт',
      Paid: 'Оплачен',
      Void: 'Аннулирован',
      Uncollectible: 'Не взыскан',
      Active: 'Активна',
      PendingActivation: 'Ожидает активации',
      Incomplete: 'Неполная активация',
      PastDue: 'Просрочена',
      Unpaid: 'Не оплачена',
      Paused: 'Приостановлена',
      Trialing: 'Пробный период',
      Revoked: 'Отозвана',
    };

    return map[status] ?? status;
  }

  getStatusClass(status: string): string {
    const normalized = status.toLowerCase();
    if (
      normalized.includes('succeeded')
      || normalized === 'active'
      || normalized === 'paid'
    ) {
      return 'status status--success';
    }
    if (normalized.includes('pending') || normalized.includes('initiated') || normalized === 'open' || normalized === 'trialing') {
      return 'status status--warning';
    }
    return 'status status--danger';
  }

  canRetryAttempt(attempt: PaymentAttemptDto): boolean {
    if (attempt.status === 'Failed' || attempt.status === 'Canceled' || attempt.status === 'Expired') {
      return true;
    }

    return this.currentAttemptState() === 'cancel' && attempt.status !== 'Succeeded';
  }

  getCurrentAttemptNotice(attempt: PaymentAttemptDto | null): string | null {
    if (!attempt) return null;

    if (attempt.status === 'Succeeded') {
      return 'Оплата подтверждена. Доступ к курсу уже выдан.';
    }

    if (this.currentAttemptState() === 'success') {
      return 'Checkout завершён. Ждём подтверждения от провайдера.';
    }

    if (this.currentAttemptState() === 'cancel') {
      return 'Checkout был отменён. Попытку можно запустить заново.';
    }

    return null;
  }

  getCurrentAttemptNoticeClass(attempt: PaymentAttemptDto | null): string {
    if (attempt?.status === 'Succeeded') {
      return 'payments-banner payments-banner--success';
    }

    return 'payments-banner payments-banner--warning';
  }

  canRetrySubscriptionAttempt(attempt: SubscriptionPaymentAttemptDto): boolean {
    if (attempt.status === 'Failed' || attempt.status === 'Canceled' || attempt.status === 'Expired') {
      return true;
    }

    return this.currentSubscriptionAttemptState() === 'cancel' && attempt.status !== 'Succeeded';
  }

  getCurrentSubscriptionAttemptNotice(attempt: SubscriptionPaymentAttemptDto | null): string | null {
    if (!attempt) return null;

    if (attempt.status === 'Succeeded') {
      return 'Подписка подтверждена.';
    }

    if (this.currentSubscriptionAttemptState() === 'success') {
      return 'Checkout завершён. Ждём подтверждения от провайдера.';
    }

    if (this.currentSubscriptionAttemptState() === 'cancel') {
      return 'Checkout подписки был отменён. Попытку можно запустить заново.';
    }

    return null;
  }

  getCurrentSubscriptionAttemptNoticeClass(attempt: SubscriptionPaymentAttemptDto | null): string {
    if (attempt?.status === 'Succeeded') {
      return 'payments-banner payments-banner--success';
    }

    return 'payments-banner payments-banner--warning';
  }

  getPlanIntervalLabel(plan: SubscriptionPlanDto): string {
    const count = plan.billingIntervalCount;
    const interval = plan.billingInterval === 'Year' ? 'год' : 'месяц';

    if (count === 1) {
      return interval === 'год' ? 'в год' : 'в месяц';
    }

    return `каждые ${count} ${interval === 'год' ? 'г.' : 'мес.'}`;
  }

  retryAttempt(attempt: PaymentAttemptDto): void {
    this.paymentsService.createCourseCheckout(attempt.courseId).subscribe({
      next: (session) => window.location.assign(session.checkoutUrl),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  startSubscriptionCheckout(subscriptionPlanId: string): void {
    this.paymentsService.createSubscriptionCheckout(subscriptionPlanId).subscribe({
      next: (session) => window.location.assign(session.checkoutUrl),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  retrySubscriptionAttempt(attempt: SubscriptionPaymentAttemptDto): void {
    this.startSubscriptionCheckout(attempt.subscriptionPlanId);
  }

  private loadPage(): void {
    this.loadCourseHistory();
    this.loadSubscriptions();
    this.loadSubscriptionHistory();
    this.loadSubscriptionInvoices();
    this.loadSubscriptionPlans();
  }

  private loadCourseHistory(): void {
    this.paymentsService.getMyPaymentHistory().subscribe({
      next: (history) => this.courseHistory.set(history),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  private loadCurrentAttempt(attemptId: string): void {
    this.loadingAttempt.set(true);
    this.paymentsService.getPaymentAttempt(attemptId).subscribe({
      next: (attempt) => {
        this.currentAttempt.set(attempt);
        this.loadingAttempt.set(false);
        this.loadCourseHistory();
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loadingAttempt.set(false);
      },
    });
  }

  private loadSubscriptions(): void {
    this.paymentsService.getMySubscriptions().subscribe({
      next: (subscriptions) => this.subscriptions.set(subscriptions),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  private loadSubscriptionHistory(): void {
    this.paymentsService.getMySubscriptionHistory().subscribe({
      next: (history) => this.subscriptionHistory.set(history),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  private loadSubscriptionInvoices(): void {
    this.paymentsService.getMySubscriptionInvoices().subscribe({
      next: (invoices) => this.subscriptionInvoices.set(invoices),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  private loadSubscriptionPlans(): void {
    this.paymentsService.getSubscriptionPlans().subscribe({
      next: (plans) => this.subscriptionPlans.set(plans),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }
}

import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { PaymentsService } from '../services/payments.service';
import {
  CoursePurchaseDto,
  DisputeRecordDto,
  PaymentAttemptDto,
  PaymentMethodRefDto,
  RefundRecordDto,
  SubscriptionInvoiceDto,
  SubscriptionPaymentAttemptDto,
  SubscriptionPlanDto,
  UserSubscriptionDto,
} from '../models/payments.model';
import { parseApiError } from '../../../core/models/api-error.model';

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

  readonly loading = signal(true);
  readonly loadingAttempt = signal(false);
  readonly error = signal<string | null>(null);
  readonly currentAttemptState = signal<'success' | 'cancel' | null>(null);
  readonly currentAttempt = signal<PaymentAttemptDto | null>(null);
  readonly currentSubscriptionAttemptState = signal<'success' | 'cancel' | null>(null);
  readonly currentSubscriptionAttempt = signal<SubscriptionPaymentAttemptDto | null>(null);
  readonly removingPaymentMethodId = signal<string | null>(null);
  readonly history = signal<PaymentAttemptDto[]>([]);
  readonly subscriptions = signal<UserSubscriptionDto[]>([]);
  readonly subscriptionHistory = signal<SubscriptionPaymentAttemptDto[]>([]);
  readonly subscriptionInvoices = signal<SubscriptionInvoiceDto[]>([]);
  readonly purchases = signal<CoursePurchaseDto[]>([]);
  readonly refunds = signal<RefundRecordDto[]>([]);
  readonly disputes = signal<DisputeRecordDto[]>([]);
  readonly paymentMethods = signal<PaymentMethodRefDto[]>([]);
  readonly subscriptionPlans = signal<SubscriptionPlanDto[]>([]);

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
        this.paymentsService.cancelPaymentAttempt(attemptId).subscribe({
          next: (attempt) => {
            this.currentAttempt.set(attempt);
            this.loadingAttempt.set(false);
            this.loadHistory();
            this.loadPurchases();
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
    return new Date(value).toLocaleString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  getStatusLabel(status: string): string {
    const map: Record<string, string> = {
      Pending: 'Ожидает',
      Initiated: 'Создана',
      PendingProvider: 'Ожидает оплату',
      Succeeded: 'Оплачено',
      Failed: 'Ошибка оплаты',
      Canceled: 'Отменено',
      Expired: 'Истекло',
      Refunded: 'Возврат',
      PartiallyRefunded: 'Частичный возврат',
      Disputed: 'Спор',
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
      Revoked: 'Доступ отозван',
      NeedsResponse: 'Нужен ответ',
      UnderReview: 'На рассмотрении',
      Won: 'Выигран',
      Lost: 'Проигран',
      WarningNeedsResponse: 'Раннее предупреждение',
      WarningUnderReview: 'Warning на рассмотрении',
      WarningClosed: 'Warning закрыт',
      Prevented: 'Предотвращён',
    };

    return map[status] ?? status;
  }

  getStatusClass(status: string): string {
    const normalized = status.toLowerCase();
    if (
      normalized.includes('success')
      || normalized.includes('succeeded')
      || normalized.includes('active')
      || normalized === 'paid'
    ) {
      return 'status status--success';
    }
    if (normalized.includes('won') || normalized.includes('prevented') || normalized.includes('closed')) {
      return 'status status--success';
    }
    if (normalized.includes('refund') || normalized === 'open' || normalized.includes('pending') || normalized.includes('initiated')) {
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
      return 'Оплата подтверждена. Доступ к курсу уже выдан или будет доступен сразу после обновления страницы.';
    }

    if (this.currentAttemptState() === 'success') {
      return 'Checkout завершён. Ждём подтверждения оплаты по webhook от провайдера. Не запускайте повторную оплату, пока статус не обновится.';
    }

    if (this.currentAttemptState() === 'cancel') {
      return 'Checkout был отменён до подтверждения оплаты. Эту попытку можно запустить заново.';
    }

    return null;
  }

  getCurrentAttemptNoticeClass(attempt: PaymentAttemptDto | null): string {
    if (attempt?.status === 'Succeeded') {
      return 'payments-card__notice payments-card__notice--success';
    }

    return 'payments-card__notice payments-card__notice--warning';
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
      return 'Подписка подтверждена. Статус активной подписки уже обновлён или подтянется после перезагрузки страницы.';
    }

    if (this.currentSubscriptionAttemptState() === 'success') {
      return 'Checkout завершён. Ждём webhook от провайдера для финального статуса подписки. Не запускайте повторное оформление, пока статус не обновится.';
    }

    if (this.currentSubscriptionAttemptState() === 'cancel') {
      return 'Checkout подписки был отменён. Эту попытку можно запустить заново.';
    }

    return null;
  }

  getCurrentSubscriptionAttemptNoticeClass(attempt: SubscriptionPaymentAttemptDto | null): string {
    if (attempt?.status === 'Succeeded') {
      return 'payments-card__notice payments-card__notice--success';
    }

    return 'payments-card__notice payments-card__notice--warning';
  }

  hasBlockingSubscription(): boolean {
    return this.subscriptions().some((subscription) => subscription.status !== 'Canceled');
  }

  getSubscriptionIntervalLabel(plan: SubscriptionPlanDto): string {
    const count = plan.billingIntervalCount;
    const interval = plan.billingInterval === 'Year' ? 'год' : 'месяц';

    if (count === 1) {
      return interval === 'год' ? 'в год' : 'в месяц';
    }

    return `каждые ${count} ${interval === 'год' ? 'г.' : 'мес.'}`;
  }

  getSubscriptionAttemptIntervalLabel(attempt: SubscriptionPaymentAttemptDto): string {
    const count = attempt.billingIntervalCount;
    const interval = attempt.billingInterval === 'Year' ? 'год' : 'месяц';

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

  removePaymentMethod(paymentMethodId: string): void {
    this.error.set(null);
    this.removingPaymentMethodId.set(paymentMethodId);

    this.paymentsService.removePaymentMethod(paymentMethodId).subscribe({
      next: () => this.loadPaymentMethods(() => this.removingPaymentMethodId.set(null)),
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.removingPaymentMethodId.set(null);
      },
    });
  }

  private loadPage(): void {
    this.loading.set(true);
    this.loadHistory();
    this.loadSubscriptions();
    this.loadSubscriptionHistory();
    this.loadSubscriptionInvoices();
    this.loadPurchases();
    this.loadRefunds();
    this.loadDisputes();
    this.loadSubscriptionPlans();
    this.loadPaymentMethods(() => this.loading.set(false));
  }

  private loadHistory(): void {
    this.paymentsService.getMyPaymentHistory().subscribe({
      next: (history) => this.history.set(history),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  private loadCurrentAttempt(attemptId: string): void {
    this.loadingAttempt.set(true);
    this.paymentsService.getPaymentAttempt(attemptId).subscribe({
      next: (attempt) => {
        this.currentAttempt.set(attempt);
        this.loadingAttempt.set(false);
        this.loadHistory();
        this.loadPurchases();
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loadingAttempt.set(false);
      },
    });
  }

  private loadPurchases(): void {
    this.paymentsService.getMyPurchases().subscribe({
      next: (purchases) => this.purchases.set(purchases),
      error: (err) => this.error.set(parseApiError(err).message),
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

  private loadRefunds(): void {
    this.paymentsService.getMyRefunds().subscribe({
      next: (refunds) => this.refunds.set(refunds),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  private loadDisputes(): void {
    this.paymentsService.getMyDisputes().subscribe({
      next: (disputes) => this.disputes.set(disputes),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  private loadPaymentMethods(onDone?: () => void): void {
    this.paymentsService.getMyPaymentMethods().subscribe({
      next: (methods) => {
        this.paymentMethods.set(methods);
        onDone?.();
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        onDone?.();
      },
    });
  }

  private loadSubscriptionPlans(): void {
    this.paymentsService.getSubscriptionPlans().subscribe({
      next: (plans) => this.subscriptionPlans.set(plans),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }
}

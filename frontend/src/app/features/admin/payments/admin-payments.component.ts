import { Component, OnInit, computed, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LucideAngularModule, Search, RotateCcw, X } from 'lucide-angular';
import { Subject, debounceTime, distinctUntilChanged, forkJoin } from 'rxjs';
import {
  AdminPaymentRecordDto,
  AdminRefundRequest,
  AdminSubscriptionAllocationRunDto,
  AdminSubscriptionPlanDto,
  UpsertSubscriptionPlanRequest,
} from '../models/admin.model';
import { AdminService, PagedResult } from '../services/admin.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';

@Component({
  selector: 'app-admin-payments',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './admin-payments.component.html',
  styleUrl: './admin-payments.component.scss',
})
export class AdminPaymentsComponent implements OnInit {
  private readonly admin = inject(AdminService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly SearchIcon = Search;
  readonly RefundIcon = RotateCcw;
  readonly XIcon = X;

  readonly searchText = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(20);

  readonly loading = signal(false);
  readonly refunding = signal(false);
  readonly savingPlan = signal(false);
  readonly data = signal<PagedResult<AdminPaymentRecordDto> | null>(null);
  readonly totalPages = computed(() => this.data()?.totalPages ?? 1);
  readonly subscriptionPlans = signal<AdminSubscriptionPlanDto[]>([]);
  readonly subscriptionAllocationRuns = signal<AdminSubscriptionAllocationRunDto[]>([]);

  readonly refundOpenFor = signal<AdminPaymentRecordDto | null>(null);
  readonly refundAmount = signal('');
  readonly refundReason = signal('');
  readonly editingPlan = signal<AdminSubscriptionPlanDto | null>(null);
  readonly planModalOpen = signal(false);
  readonly planName = signal('');
  readonly planDescription = signal('');
  readonly planPrice = signal('29.00');
  readonly planCurrency = signal('usd');
  readonly planBillingInterval = signal('Month');
  readonly planBillingIntervalCount = signal('1');
  readonly planIsActive = signal(true);
  readonly planIsFeatured = signal(false);
  readonly planSortOrder = signal('0');
  readonly planProviderProductId = signal('');
  readonly planProviderPriceId = signal('');

  private readonly search$ = new Subject<string>();

  ngOnInit(): void {
    this.search$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.load());

    this.loadPage();
  }

  onSearchChange(): void {
    this.page.set(1);
    this.search$.next(this.searchText().trim());
  }

  prevPage(): void {
    if (this.page() > 1) {
      this.page.update(v => v - 1);
      this.load();
    }
  }

  nextPage(): void {
    if (this.page() < this.totalPages()) {
      this.page.update(v => v + 1);
      this.load();
    }
  }

  load(): void {
    this.loading.set(true);
    this.admin.getPayments({
      search: this.searchText() || undefined,
      page: this.page(),
      pageSize: this.pageSize(),
    }).subscribe({
      next: (data) => {
        this.data.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.toast.error(parseApiError(err).message);
        this.loading.set(false);
      },
    });
  }

  openCreatePlan(): void {
    this.editingPlan.set(null);
    this.planName.set('');
    this.planDescription.set('');
    this.planPrice.set('29.00');
    this.planCurrency.set('usd');
    this.planBillingInterval.set('Month');
    this.planBillingIntervalCount.set('1');
    this.planIsActive.set(true);
    this.planIsFeatured.set(false);
    this.planSortOrder.set(String(this.subscriptionPlans().length));
    this.planProviderProductId.set('');
    this.planProviderPriceId.set('');
    this.planModalOpen.set(true);
  }

  openEditPlan(plan: AdminSubscriptionPlanDto): void {
    this.editingPlan.set(plan);
    this.planName.set(plan.name);
    this.planDescription.set(plan.description ?? '');
    this.planPrice.set(plan.price.toFixed(2));
    this.planCurrency.set(plan.currency);
    this.planBillingInterval.set(plan.billingInterval);
    this.planBillingIntervalCount.set(String(plan.billingIntervalCount));
    this.planIsActive.set(plan.isActive);
    this.planIsFeatured.set(plan.isFeatured);
    this.planSortOrder.set(String(plan.sortOrder));
    this.planProviderProductId.set(plan.providerProductId ?? '');
    this.planProviderPriceId.set(plan.providerPriceId ?? '');
    this.planModalOpen.set(true);
  }

  cancelPlanModal(): void {
    this.planModalOpen.set(false);
    this.editingPlan.set(null);
  }

  submitPlan(): void {
    const price = Number(this.planPrice().replace(',', '.'));
    const billingIntervalCount = Number(this.planBillingIntervalCount());
    const sortOrder = Number(this.planSortOrder());

    if (!this.planName().trim()) {
      this.toast.warning('Укажите название тарифа');
      return;
    }

    if (!Number.isFinite(price) || price <= 0) {
      this.toast.warning('Укажите корректную стоимость тарифа');
      return;
    }

    if (!Number.isInteger(billingIntervalCount) || billingIntervalCount <= 0) {
      this.toast.warning('Интервал тарифа должен быть целым числом больше нуля');
      return;
    }

    if (!Number.isInteger(sortOrder)) {
      this.toast.warning('Sort order должен быть целым числом');
      return;
    }

    const request: UpsertSubscriptionPlanRequest = {
      name: this.planName().trim(),
      description: this.planDescription().trim() || undefined,
      price,
      currency: this.planCurrency().trim() || 'usd',
      billingInterval: this.planBillingInterval(),
      billingIntervalCount,
      isActive: this.planIsActive(),
      isFeatured: this.planIsFeatured(),
      sortOrder,
      providerProductId: this.planProviderProductId().trim() || undefined,
      providerPriceId: this.planProviderPriceId().trim() || undefined,
    };

    const editing = this.editingPlan();
    this.savingPlan.set(true);
    const request$ = editing
      ? this.admin.updateSubscriptionPlan(editing.id, request)
      : this.admin.createSubscriptionPlan(request);

    request$.subscribe({
      next: () => {
        this.savingPlan.set(false);
        this.toast.success(editing ? 'Тариф обновлён' : 'Тариф создан');
        this.cancelPlanModal();
        this.loadPage();
      },
      error: (err) => {
        this.savingPlan.set(false);
        this.toast.error(parseApiError(err).message);
      },
    });
  }

  openRefund(item: AdminPaymentRecordDto): void {
    this.refundOpenFor.set(item);
    this.refundAmount.set(item.remainingRefundableAmount.toFixed(2));
    this.refundReason.set('');
  }

  cancelRefund(): void {
    this.refundOpenFor.set(null);
    this.refundAmount.set('');
    this.refundReason.set('');
  }

  submitRefund(): void {
    const payment = this.refundOpenFor();
    if (!payment) return;

    const amount = Number(this.refundAmount().replace(',', '.'));
    if (!Number.isFinite(amount) || amount <= 0) {
      this.toast.warning('Укажите корректную сумму refund');
      return;
    }
    if (amount > payment.remainingRefundableAmount) {
      this.toast.warning('Сумма refund превышает доступный остаток');
      return;
    }

    const request: AdminRefundRequest = {
      paymentAttemptId: payment.paymentAttemptId,
      amount,
      reason: this.refundReason().trim() || undefined,
    };

    this.refunding.set(true);
    this.admin.createRefund(request).subscribe({
      next: () => {
        this.refunding.set(false);
        this.toast.success('Refund создан');
        this.cancelRefund();
        this.load();
      },
      error: (err) => {
        this.refunding.set(false);
        this.toast.error(parseApiError(err).message);
      },
    });
  }

  formatAmount(amount: number, currency: string): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: currency.toUpperCase(),
      maximumFractionDigits: 2,
    }).format(amount);
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  paymentStatusLabel(status: string): string {
    const map: Record<string, string> = {
      Initiated: 'Создана',
      PendingProvider: 'Ожидает оплату',
      Succeeded: 'Оплачено',
      Failed: 'Ошибка',
      Canceled: 'Отменено',
      Expired: 'Истекло',
      Refunded: 'Возвращено',
      PartiallyRefunded: 'Частичный возврат',
      Disputed: 'Спор',
    };
    return map[status] ?? status;
  }

  paymentStatusClass(status: string): string {
    const normalized = status.toLowerCase();
    if (normalized.includes('succeeded')) return 'badge badge--ok';
    if (normalized.includes('disput')) return 'badge badge--danger';
    if (normalized.includes('refund')) return 'badge badge--warn';
    if (normalized.includes('pending') || normalized.includes('initiated')) return 'badge badge--neutral';
    return 'badge badge--danger';
  }

  disputeStatusLabel(status?: string | null): string {
    const map: Record<string, string> = {
      NeedsResponse: 'Нужен ответ',
      UnderReview: 'На рассмотрении',
      Won: 'Выигран',
      Lost: 'Проигран',
      WarningNeedsResponse: 'Раннее предупреждение',
      WarningUnderReview: 'Warning на рассмотрении',
      WarningClosed: 'Warning закрыт',
      Prevented: 'Предотвращён',
    };
    return status ? map[status] ?? status : '';
  }

  disputeStatusClass(status?: string | null): string {
    if (!status) return 'badge badge--neutral';
    if (status === 'Won' || status === 'WarningClosed' || status === 'Prevented') return 'badge badge--ok';
    if (status === 'NeedsResponse' || status === 'WarningNeedsResponse') return 'badge badge--warn';
    if (status === 'UnderReview' || status === 'WarningUnderReview') return 'badge badge--neutral';
    return 'badge badge--danger';
  }

  subscriptionIntervalLabel(plan: AdminSubscriptionPlanDto): string {
    const count = plan.billingIntervalCount;
    const interval = plan.billingInterval === 'Year' ? 'год' : 'месяц';

    if (count === 1) {
      return interval === 'год' ? 'в год' : 'в месяц';
    }

    return `каждые ${count} ${interval === 'год' ? 'г.' : 'мес.'}`;
  }

  allocationStatusLabel(status: string): string {
    const map: Record<string, string> = {
      Applied: 'Распределено',
      Skipped: 'Пропущено',
      Reversed: 'Сторнировано',
      Canceled: 'Отменено',
    };
    return map[status] ?? status;
  }

  allocationStatusClass(status: string): string {
    if (status === 'Applied') return 'badge badge--ok';
    if (status === 'Skipped') return 'badge badge--warn';
    return 'badge badge--danger';
  }

  private loadPage(): void {
    this.loading.set(true);
    this.errorHandledLoad();
  }

  private errorHandledLoad(): void {
    forkJoin({
      plans: this.admin.getSubscriptionPlans(),
      allocationRuns: this.admin.getSubscriptionAllocationRuns(),
    }).subscribe({
      next: ({ plans, allocationRuns }) => {
        this.subscriptionPlans.set(plans);
        this.subscriptionAllocationRuns.set(allocationRuns);
        this.load();
      },
      error: (err) => {
        this.toast.error(parseApiError(err).message);
        this.loading.set(false);
      },
    });
  }
}

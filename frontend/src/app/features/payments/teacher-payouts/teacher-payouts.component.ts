import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { PaymentsService } from '../services/payments.service';
import { TeacherPayoutAccountDto, TeacherSettlementDto, TeacherSettlementSummaryDto } from '../models/payments.model';
import { parseApiError } from '../../../core/models/api-error.model';
import { forkJoin } from 'rxjs';

interface CourseEarningsRow {
  courseId: string;
  courseTitle: string;
  purchasesCount: number;
  grossAmount: number;
  netAmount: number;
  readyForPayoutAmount: number;
  currency: string;
}

@Component({
  selector: 'app-teacher-payouts',
  standalone: true,
  imports: [CommonModule, ButtonComponent],
  templateUrl: './teacher-payouts.component.html',
  styleUrl: './teacher-payouts.component.scss',
})
export class TeacherPayoutsComponent implements OnInit {
  private readonly paymentsService = inject(PaymentsService);

  readonly loading = signal(true);
  readonly connecting = signal(false);
  readonly openingDashboard = signal(false);
  readonly requestingPayout = signal(false);
  readonly error = signal<string | null>(null);
  readonly account = signal<TeacherPayoutAccountDto | null>(null);
  readonly summary = signal<TeacherSettlementSummaryDto | null>(null);
  readonly settlements = signal<TeacherSettlementDto[]>([]);
  readonly totalFees = computed(() => {
    const summary = this.summary();
    return summary ? Math.max(0, summary.totalGrossAmount - summary.totalNetAmount) : 0;
  });
  readonly canRequestPayout = computed(() => {
    const summary = this.summary();
    const account = this.account();
    return !!summary
      && summary.readyForPayoutNetAmount > 0
      && !!account
      && account.payoutsEnabled;
  });
  readonly courseRows = computed<CourseEarningsRow[]>(() => {
    const rows = new Map<string, CourseEarningsRow>();

    for (const settlement of this.settlements()) {
      const row = rows.get(settlement.courseId) ?? {
        courseId: settlement.courseId,
        courseTitle: settlement.courseTitle,
        purchasesCount: 0,
        grossAmount: 0,
        netAmount: 0,
        readyForPayoutAmount: 0,
        currency: settlement.currency,
      };

      row.purchasesCount += 1;
      row.grossAmount += settlement.grossAmount;
      row.netAmount += settlement.netAmount;
      if (settlement.status === 'ReadyForPayout') {
        row.readyForPayoutAmount += settlement.netAmount;
      }

      rows.set(settlement.courseId, row);
    }

    return [...rows.values()]
      .sort((a, b) => b.netAmount - a.netAmount || a.courseTitle.localeCompare(b.courseTitle));
  });

  ngOnInit(): void {
    this.loadPage();
  }

  connectPayouts(): void {
    this.connecting.set(true);
    this.paymentsService.createTeacherOnboardingLink().subscribe({
      next: ({ url }) => window.location.assign(url),
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.connecting.set(false);
      },
    });
  }

  openDashboard(): void {
    this.openingDashboard.set(true);
    this.paymentsService.createTeacherDashboardLink().subscribe({
      next: ({ url }) => window.location.assign(url),
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.openingDashboard.set(false);
      },
    });
  }

  requestPayout(): void {
    this.requestingPayout.set(true);
    this.paymentsService.requestTeacherPayout().subscribe({
      next: () => {
        this.requestingPayout.set(false);
        this.loadPage();
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.requestingPayout.set(false);
      },
    });
  }

  getStatusLabel(status: string): string {
    const map: Record<string, string> = {
      NotStarted: 'Не начато',
      OnboardingStarted: 'Онбординг начат',
      PendingVerification: 'Проверка данных',
      Ready: 'Готово к выплатам',
      Restricted: 'Требуются действия',
      Rejected: 'Отклонено',
    };

    return map[status] ?? status;
  }

  getStatusClass(status: string): string {
    if (status === 'Ready') return 'status status--success';
    if (status === 'PendingVerification' || status === 'OnboardingStarted') {
      return 'status status--warning';
    }
    return 'status status--danger';
  }

  getSettlementStatusLabel(status: string): string {
    const map: Record<string, string> = {
      PendingHold: 'Ожидает',
      ReadyForPayout: 'Доступно',
      InPayout: 'Запрошено',
      PaidOut: 'Выплачено',
      Reversed: 'Сторнировано',
      Canceled: 'Отменено',
    };

    return map[status] ?? status;
  }

  getSettlementStatusClass(status: string): string {
    if (status === 'PaidOut') return 'status status--success';
    if (status === 'ReadyForPayout') return 'status status--primary';
    if (status === 'InPayout') return 'status status--info';
    if (status === 'PendingHold') return 'status status--warning';
    return 'status status--danger';
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

  getSettlementAvailabilityText(settlement: TeacherSettlementDto): string {
    if (settlement.status === 'ReadyForPayout') {
      return 'Можно вывести сейчас';
    }

    if (settlement.status === 'PendingHold') {
      return `Будет доступно ${this.formatDate(settlement.availableAt)}`;
    }

    if (settlement.status === 'InPayout') {
      return 'Выплата уже запрошена';
    }

    if (settlement.status === 'PaidOut' && settlement.paidOutAt) {
      return `Выплачено ${this.formatDate(settlement.paidOutAt)}`;
    }

    return this.getSettlementStatusLabel(settlement.status);
  }

  getPayoutSetupText(account: TeacherPayoutAccountDto): string {
    if (!account.providerConfigured) {
      return 'Платёжная система ещё не настроена.';
    }

    if (account.status === 'Ready' && account.payoutsEnabled) {
      return 'Выплаты подключены. Можно выводить доступные начисления.';
    }

    if (account.status === 'PendingVerification' || account.status === 'OnboardingStarted') {
      return 'Данные отправлены, платёжная система проверяет подключение.';
    }

    return 'Чтобы получать выплаты, завершите подключение платёжного аккаунта.';
  }

  private loadPage(): void {
    this.loading.set(true);
    forkJoin({
      account: this.paymentsService.getTeacherPayoutAccount(),
      summary: this.paymentsService.getTeacherSettlementSummary(),
      settlements: this.paymentsService.getTeacherSettlements(),
    }).subscribe({
      next: ({ account, summary, settlements }) => {
        this.account.set(account);
        this.summary.set(summary);
        this.settlements.set(settlements);
        this.loading.set(false);
        this.connecting.set(false);
        this.openingDashboard.set(false);
        this.requestingPayout.set(false);
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loading.set(false);
        this.connecting.set(false);
        this.openingDashboard.set(false);
        this.requestingPayout.set(false);
      },
    });
  }
}

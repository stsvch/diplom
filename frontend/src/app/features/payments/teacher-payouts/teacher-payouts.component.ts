import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { PaymentsService } from '../services/payments.service';
import {
  DisputeRecordDto,
  PayoutRecordDto,
  TeacherPayoutAccountDto,
  TeacherSettlementDto,
  TeacherSettlementSummaryDto,
  TeacherSubscriptionAllocationDto,
} from '../models/payments.model';
import { parseApiError } from '../../../core/models/api-error.model';
import { forkJoin } from 'rxjs';

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
  readonly subscriptionAllocations = signal<TeacherSubscriptionAllocationDto[]>([]);
  readonly disputes = signal<DisputeRecordDto[]>([]);
  readonly payouts = signal<PayoutRecordDto[]>([]);
  readonly subscriptionAllocationNetTotal = computed(() =>
    this.subscriptionAllocations()
      .filter((item) => item.status === 'Applied')
      .reduce((sum, item) => sum + item.netAmount, 0),
  );

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
      PendingHold: 'Холд',
      ReadyForPayout: 'Готово к выплате',
      InPayout: 'В payout batch',
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

  getPayoutStatusLabel(status: string): string {
    const map: Record<string, string> = {
      Queued: 'В очереди',
      SubmittedToProvider: 'Transfer отправлен',
      Paid: 'Переведено преподавателю',
      Failed: 'Ошибка выплаты',
      Reversed: 'Сторнировано',
      Canceled: 'Отменено',
    };

    return map[status] ?? status;
  }

  getPayoutStatusClass(status: string): string {
    if (status === 'Paid') return 'status status--success';
    if (status === 'Queued' || status === 'SubmittedToProvider') return 'status status--info';
    return 'status status--danger';
  }

  getAllocationStatusLabel(status: string): string {
    const map: Record<string, string> = {
      Applied: 'Распределено',
      Skipped: 'Пропущено',
      Reversed: 'Сторнировано',
      Canceled: 'Отменено',
    };

    return map[status] ?? status;
  }

  getAllocationStatusClass(status: string): string {
    if (status === 'Applied') return 'status status--success';
    if (status === 'Skipped') return 'status status--warning';
    return 'status status--danger';
  }

  getAllocationPayoutStatusLabel(status: string): string {
    const map: Record<string, string> = {
      PendingHold: 'В холде',
      ReadyForPayout: 'Готово к выплате',
      InPayout: 'В payout batch',
      PaidOut: 'Выплачено',
      Reversed: 'Сторнировано',
      Canceled: 'Отменено',
      Skipped: 'Не распределяется',
    };

    return map[status] ?? status;
  }

  getAllocationPayoutStatusClass(status: string): string {
    if (status === 'PaidOut') return 'status status--success';
    if (status === 'ReadyForPayout') return 'status status--primary';
    if (status === 'InPayout') return 'status status--info';
    if (status === 'PendingHold' || status === 'Skipped') return 'status status--warning';
    return 'status status--danger';
  }

  getDisputeStatusLabel(status: string): string {
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

    return map[status] ?? status;
  }

  getDisputeStatusClass(status: string): string {
    if (status === 'Won' || status === 'WarningClosed' || status === 'Prevented') {
      return 'status status--success';
    }
    if (status === 'NeedsResponse' || status === 'WarningNeedsResponse') {
      return 'status status--warning';
    }
    if (status === 'UnderReview' || status === 'WarningUnderReview') {
      return 'status status--info';
    }
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

  private loadPage(): void {
    this.loading.set(true);
    forkJoin({
      account: this.paymentsService.getTeacherPayoutAccount(),
      summary: this.paymentsService.getTeacherSettlementSummary(),
      settlements: this.paymentsService.getTeacherSettlements(),
      subscriptionAllocations: this.paymentsService.getTeacherSubscriptionAllocations(),
      disputes: this.paymentsService.getTeacherDisputes(),
      payouts: this.paymentsService.getTeacherPayoutRecords(),
    }).subscribe({
      next: ({ account, summary, settlements, subscriptionAllocations, disputes, payouts }) => {
        this.account.set(account);
        this.summary.set(summary);
        this.settlements.set(settlements);
        this.subscriptionAllocations.set(subscriptionAllocations);
        this.disputes.set(disputes);
        this.payouts.set(payouts);
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

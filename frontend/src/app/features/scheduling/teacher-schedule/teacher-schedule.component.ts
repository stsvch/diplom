import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  Plus,
  Users,
  User,
  Calendar,
  Clock,
  Link,
  Trash2,
  Loader2,
  CalendarRange,
  Repeat,
} from 'lucide-angular';
import { SchedulingService } from '../services/scheduling.service';
import {
  TeacherAvailabilityDto,
  CreateAvailabilityRequest,
  AvailabilityKind,
  SessionType,
  DayOfWeek,
} from '../models/scheduling.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { parseApiError } from '../../../core/models/api-error.model';

interface AvailabilityForm {
  kind: AvailabilityKind;
  dayOfWeek: DayOfWeek;
  specificDate: string;
  startTime: string;
  endTime: string;
  slotDurationMinutes: number;
  breakBetweenMinutes: number;
  validFrom: string;
  validUntil: string;
  sessionType: SessionType;
  maxStudents: number;
  title: string;
  description: string;
  meetingLink: string;
}

@Component({
  selector: 'app-teacher-schedule',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, ButtonComponent, BadgeComponent],
  templateUrl: './teacher-schedule.component.html',
  styleUrl: './teacher-schedule.component.scss',
})
export class TeacherScheduleComponent implements OnInit {
  private readonly schedulingService = inject(SchedulingService);
  private readonly toastService = inject(ToastService);

  readonly icons = {
    plus: Plus,
    users: Users,
    user: User,
    calendar: Calendar,
    clock: Clock,
    link: Link,
    trash: Trash2,
    loader: Loader2,
    range: CalendarRange,
    repeat: Repeat,
  };

  readonly Kind = AvailabilityKind;
  readonly SessionT = SessionType;

  readonly daysOfWeek: { value: DayOfWeek; label: string }[] = [
    { value: 'Monday', label: 'Пн' },
    { value: 'Tuesday', label: 'Вт' },
    { value: 'Wednesday', label: 'Ср' },
    { value: 'Thursday', label: 'Чт' },
    { value: 'Friday', label: 'Пт' },
    { value: 'Saturday', label: 'Сб' },
    { value: 'Sunday', label: 'Вс' },
  ];

  readonly rules = signal<TeacherAvailabilityDto[]>([]);
  readonly loading = signal(false);
  readonly creating = signal(false);
  readonly deletingId = signal<string | null>(null);
  readonly showCreateModal = signal(false);

  private defaultForm(): AvailabilityForm {
    const today = new Date();
    const todayStr = today.toISOString().slice(0, 10);
    return {
      kind: AvailabilityKind.Recurring,
      dayOfWeek: 'Monday',
      specificDate: todayStr,
      startTime: '17:00',
      endTime: '21:00',
      slotDurationMinutes: 60,
      breakBetweenMinutes: 0,
      validFrom: todayStr,
      validUntil: '',
      sessionType: SessionType.Individual,
      maxStudents: 1,
      title: '',
      description: '',
      meetingLink: '',
    };
  }

  readonly form = signal<AvailabilityForm>(this.defaultForm());

  readonly recurringRules = computed(() =>
    this.rules().filter((r) => r.kind === AvailabilityKind.Recurring),
  );
  readonly oneOffRules = computed(() =>
    this.rules().filter((r) => r.kind === AvailabilityKind.OneOff),
  );

  /** Часовая шкала для timeline (06:00 → 23:00, 17 шагов = 18 точек). */
  readonly timelineHours: readonly number[] = Array.from({ length: 18 }, (_, i) => i + 6);
  readonly timelineStartMin = 6 * 60;   // 06:00
  readonly timelineEndMin = 24 * 60;    // 24:00 (исключительно)

  /** Уже занятые интервалы на выбранный день / дату из формы. */
  readonly busyIntervalsForFormDay = computed(() => {
    const f = this.form();
    return this.rules()
      .filter((r) => r.isActive && this.ruleAffectsFormDay(r, f))
      .map((r) => ({
        title: r.title,
        startMin: this.timeToMinutes(r.startTime),
        endMin: this.timeToMinutes(r.endTime),
      }));
  });

  /** Текущий интервал в форме (для подсветки в timeline). */
  readonly formInterval = computed(() => {
    const f = this.form();
    const start = this.hhmmToMinutes(f.startTime);
    const end = this.hhmmToMinutes(f.endTime);
    if (end <= start) return null;
    return { startMin: start, endMin: end };
  });

  /** Найденный конфликт правил (для UI-предупреждения и блокировки кнопки). */
  readonly formConflict = computed(() => {
    const interval = this.formInterval();
    if (!interval) return null;
    const busy = this.busyIntervalsForFormDay();
    return busy.find(
      (b) => b.startMin < interval.endMin && interval.startMin < b.endMin,
    ) ?? null;
  });

  // helpers ─────────────
  toPercent(min: number): number {
    const total = this.timelineEndMin - this.timelineStartMin;
    return Math.max(0, Math.min(100, ((min - this.timelineStartMin) / total) * 100));
  }
  intervalWidth(startMin: number, endMin: number): number {
    return this.toPercent(endMin) - this.toPercent(startMin);
  }
  formatHour(h: number): string {
    return `${h.toString().padStart(2, '0')}:00`;
  }
  formatMinutes(min: number): string {
    const h = Math.floor(min / 60).toString().padStart(2, '0');
    const m = (min % 60).toString().padStart(2, '0');
    return `${h}:${m}`;
  }

  private timeToMinutes(t: string): number {
    // 'HH:mm:ss' или 'HH:mm'
    const [h, m] = t.split(':').map(Number);
    return h * 60 + (m || 0);
  }
  private hhmmToMinutes(t: string): number {
    if (!t) return 0;
    const [h, m] = t.split(':').map(Number);
    return h * 60 + (m || 0);
  }
  private ruleAffectsFormDay(r: TeacherAvailabilityDto, f: AvailabilityForm): boolean {
    if (f.kind === AvailabilityKind.Recurring) {
      if (r.kind === AvailabilityKind.Recurring) return r.dayOfWeek === f.dayOfWeek;
      // OneOff пересекается с recurring если день недели OneOff.SpecificDate == f.dayOfWeek
      if (r.specificDate) {
        const dow = this.dateOnlyToDayOfWeek(r.specificDate);
        return dow === f.dayOfWeek;
      }
      return false;
    }
    // f — OneOff
    if (!f.specificDate) return false;
    if (r.kind === AvailabilityKind.OneOff) return r.specificDate === f.specificDate;
    // r — recurring: совпадает если день недели f.specificDate == r.dayOfWeek
    return r.dayOfWeek === this.dateOnlyToDayOfWeek(f.specificDate);
  }
  private dateOnlyToDayOfWeek(d: string): DayOfWeek {
    const day = new Date(d + 'T12:00:00Z').getUTCDay();
    const map: DayOfWeek[] = ['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'];
    return map[day];
  }

  ngOnInit(): void {
    this.loadRules();
  }

  private loadRules(): void {
    this.loading.set(true);
    this.schedulingService.getMyAvailability().subscribe({
      next: (rules) => {
        this.rules.set(rules);
        this.loading.set(false);
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.loading.set(false);
      },
    });
  }

  openCreateModal(): void {
    this.form.set(this.defaultForm());
    this.showCreateModal.set(true);
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
  }

  updateForm<K extends keyof AvailabilityForm>(key: K, value: AvailabilityForm[K]): void {
    this.form.update((f) => {
      const next = { ...f, [key]: value };
      if (key === 'sessionType' && value === SessionType.Individual) {
        next.maxStudents = 1;
      }
      return next;
    });
  }

  createRule(): void {
    const f = this.form();
    if (!f.title.trim()) {
      this.toastService.error('Укажите название.');
      return;
    }
    if (f.startTime >= f.endTime) {
      this.toastService.error('Конец интервала должен быть позже начала.');
      return;
    }
    if (f.slotDurationMinutes <= 0) {
      this.toastService.error('Длительность занятия должна быть больше нуля.');
      return;
    }

    const request: CreateAvailabilityRequest = {
      kind: f.kind,
      dayOfWeek: f.kind === AvailabilityKind.Recurring ? f.dayOfWeek : undefined,
      specificDate: f.kind === AvailabilityKind.OneOff ? f.specificDate : undefined,
      startTime: this.toFullTime(f.startTime),
      endTime: this.toFullTime(f.endTime),
      slotDurationMinutes: f.slotDurationMinutes,
      breakBetweenMinutes: f.breakBetweenMinutes,
      validFrom: f.validFrom,
      validUntil: f.validUntil || undefined,
      sessionType: f.sessionType,
      maxStudents: f.sessionType === SessionType.Individual ? 1 : f.maxStudents,
      title: f.title.trim(),
      description: f.description.trim() || undefined,
      meetingLink: f.meetingLink.trim() || undefined,
    };

    this.creating.set(true);
    this.schedulingService.createAvailability(request).subscribe({
      next: (created) => {
        this.rules.update((list) => [created, ...list]);
        this.creating.set(false);
        this.showCreateModal.set(false);
        this.toastService.success('Расписание добавлено.');
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.creating.set(false);
      },
    });
  }

  deleteRule(rule: TeacherAvailabilityDto): void {
    if (!confirm(`Удалить расписание «${rule.title}»?`)) return;

    this.deletingId.set(rule.id);
    this.schedulingService.deleteAvailability(rule.id).subscribe({
      next: (resp) => {
        this.deletingId.set(null);
        this.toastService.success(resp.message);
        this.loadRules();
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.deletingId.set(null);
      },
    });
  }

  formatDayOfWeek(d: DayOfWeek | undefined): string {
    return this.daysOfWeek.find((x) => x.value === d)?.label ?? '';
  }

  formatTime(t: string): string {
    return t?.slice(0, 5) ?? '';
  }

  formatDate(d: string | undefined): string {
    if (!d) return '';
    return new Date(d).toLocaleDateString('ru-RU', { day: '2-digit', month: 'long', year: 'numeric' });
  }

  private toFullTime(hhmm: string): string {
    return hhmm.length === 5 ? `${hhmm}:00` : hhmm;
  }
}

// student-schedule.component.ts
import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  Calendar,
  Clock,
  Users,
  User,
  Search,
  CheckCircle,
  X,
  Loader2,
  ArrowLeft,
  Sparkles,
} from 'lucide-angular';
import { SchedulingService } from '../services/scheduling.service';
import {
  CalendarSlotDto,
  ScheduleSlotDto,
  SessionType,
  TeacherWithScheduleDto,
} from '../models/scheduling.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { parseApiError } from '../../../core/models/api-error.model';
import { PaymentsService } from '../../payments/services/payments.service';
import { UserEntitlementsDto } from '../../payments/models/payments.model';

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-student-schedule',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, ButtonComponent, BadgeComponent],
  templateUrl: './student-schedule.component.html',
  styleUrl: './student-schedule.component.scss',
})
export class StudentScheduleComponent implements OnInit {
  private readonly schedulingService = inject(SchedulingService);
  private readonly paymentsService = inject(PaymentsService);
  private readonly toastService = inject(ToastService);

  readonly icons = {
    calendar: Calendar,
    clock: Clock,
    users: Users,
    user: User,
    search: Search,
    check: CheckCircle,
    x: X,
    loader: Loader2,
    back: ArrowLeft,
    sparkles: Sparkles,
  };

  readonly SessionT = SessionType;

  // Signals и computed-значения хранят реактивное состояние без ручной синхронизации с шаблоном.
  readonly entitlements = signal<UserEntitlementsDto | null>(null);
  readonly teachers = signal<TeacherWithScheduleDto[]>([]);
  readonly bookings = signal<ScheduleSlotDto[]>([]);
  readonly calendar = signal<CalendarSlotDto[]>([]);

  readonly selectedTeacher = signal<TeacherWithScheduleDto | null>(null);
  readonly searchQuery = signal('');
  readonly activeTab = signal<'browse' | 'mine'>('browse');

  readonly loadingTeachers = signal(false);
  readonly loadingCalendar = signal(false);
  readonly loadingBookings = signal(false);
  readonly bookingSlot = signal<string | null>(null);
  readonly cancellingSlot = signal<string | null>(null);

  readonly filteredTeachers = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();
    const list = this.teachers();
    return q ? list.filter((t) => t.teacherName.toLowerCase().includes(q)) : list;
  });

  readonly groupedCalendar = computed(() => {
    const map = new Map<string, CalendarSlotDto[]>();
    for (const slot of this.calendar()) {
      const dateKey = slot.startTime.slice(0, 10);
      const arr = map.get(dateKey) ?? [];
      arr.push(slot);
      map.set(dateKey, arr);
    }
    return Array.from(map.entries())
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([date, slots]) => ({ date, slots }));
  });

  // Lifecycle hook запускает первичную загрузку или очистку ресурсов компонента.
  ngOnInit(): void {
    this.loadEntitlements();
    this.loadTeachers();
    this.loadMyBookings();
  }

  private loadEntitlements(): void {
    // Подписка синхронизирует ответ сервиса с локальным состоянием и уведомлениями.
    this.paymentsService.getMyEntitlements().subscribe({
      next: (e) => this.entitlements.set(e),
      error: () => { /* фича не критична */ },
    });
  }

  private loadTeachers(): void {
    this.loadingTeachers.set(true);
    this.schedulingService.getTeachersWithSchedule().subscribe({
      next: (list) => { this.teachers.set(list); this.loadingTeachers.set(false); },
      error: (err) => { this.toastService.error(parseApiError(err).message); this.loadingTeachers.set(false); },
    });
  }

  private loadMyBookings(): void {
    this.loadingBookings.set(true);
    this.schedulingService.getMyBookings().subscribe({
      next: (list) => { this.bookings.set(list); this.loadingBookings.set(false); },
      error: (err) => { this.toastService.error(parseApiError(err).message); this.loadingBookings.set(false); },
    });
  }

  selectTeacher(teacher: TeacherWithScheduleDto): void {
    this.selectedTeacher.set(teacher);
    this.loadCalendar(teacher.teacherId);
  }

  backToList(): void {
    this.selectedTeacher.set(null);
    this.calendar.set([]);
  }

  setTab(tab: 'browse' | 'mine'): void {
    this.activeTab.set(tab);
  }

  private loadCalendar(teacherId: string): void {
    this.loadingCalendar.set(true);
    const today = new Date();
    const from = today.toISOString().slice(0, 10);
    const to = new Date(today.getTime() + 28 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10);
    this.schedulingService.getTeacherCalendar(teacherId, from, to).subscribe({
      next: (slots) => { this.calendar.set(slots); this.loadingCalendar.set(false); },
      error: (err) => { this.toastService.error(parseApiError(err).message); this.loadingCalendar.set(false); },
    });
  }

  bookSlot(slot: CalendarSlotDto): void {
    if (slot.isBookedByCurrentUser) return;
    if (!slot.isAvailable) return;

    this.bookingSlot.set(slot.startTime);
    this.schedulingService.bookSlot({
      availabilityId: slot.availabilityId,
      startTime: slot.startTime,
    }).subscribe({
      next: (resp) => {
        this.bookingSlot.set(null);
        this.toastService.success(resp.message);
        if (this.selectedTeacher()) this.loadCalendar(this.selectedTeacher()!.teacherId);
        this.loadMyBookings();
        this.loadEntitlements();
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.bookingSlot.set(null);
      },
    });
  }

  cancelBooking(slot: ScheduleSlotDto): void {
    if (!confirm('Отменить запись на это занятие?')) return;
    this.cancellingSlot.set(slot.id);
    this.schedulingService.cancelBooking(slot.id).subscribe({
      next: (resp) => {
        this.cancellingSlot.set(null);
        this.toastService.success(resp.message);
        this.loadMyBookings();
        this.loadEntitlements();
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.cancellingSlot.set(null);
      },
    });
  }

  formatTime(iso: string): string {
    return new Date(iso).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
  }

  formatDateLong(date: string): string {
    return new Date(date).toLocaleDateString('ru-RU', {
      weekday: 'short', day: '2-digit', month: 'long',
    });
  }

  formatDayShort(iso: string): string {
    return new Date(iso).toLocaleDateString('ru-RU', { day: '2-digit', month: 'short' });
  }
}

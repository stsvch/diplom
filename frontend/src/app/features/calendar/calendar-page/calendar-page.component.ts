import { Component, OnInit, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LucideAngularModule, ChevronLeft, ChevronRight, Calendar, Plus, Trash2, ArrowUpRight } from 'lucide-angular';
import { CalendarService } from '../services/calendar.service';
import { CalendarEventDto, CalendarEventType, DeadlineStatus } from '../models/calendar.model';
import { parseApiError } from '../../../core/models/api-error.model';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user.model';

interface CalendarDay {
  date: Date;
  isCurrentMonth: boolean;
  isToday: boolean;
  events: CalendarEventDto[];
}

interface CreateEventForm {
  title: string;
  description: string;
  eventDate: string;
  eventTime: string;
}

@Component({
  selector: 'app-calendar-page',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, BadgeComponent],
  templateUrl: './calendar-page.component.html',
  styleUrl: './calendar-page.component.scss',
})
export class CalendarPageComponent implements OnInit {
  private readonly calendarService = inject(CalendarService);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  readonly icons = { ChevronLeft, ChevronRight, Calendar, Plus, Trash2, ArrowUpRight };

  readonly userRole = this.authService.userRole;
  readonly isTeacher = computed(() => this.userRole() === UserRole.Teacher);

  readonly currentDate = signal(this.startOfDay(new Date()));
  readonly selectedDate = signal(this.startOfDay(new Date()));
  readonly monthEvents = signal<CalendarEventDto[]>([]);
  readonly upcomingEvents = signal<CalendarEventDto[]>([]);
  readonly loading = signal(false);
  readonly creating = signal(false);
  readonly deletingId = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly actionError = signal<string | null>(null);

  readonly CalendarEventType = CalendarEventType;
  readonly DeadlineStatus = DeadlineStatus;

  readonly monthNames = [
    'Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
    'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь',
  ];

  readonly weekDays = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'];

  readonly currentMonthLabel = computed(() => {
    const d = this.currentDate();
    return `${this.monthNames[d.getMonth()]} ${d.getFullYear()}`;
  });

  readonly selectedDayLabel = computed(() =>
    this.selectedDate().toLocaleDateString('ru-RU', {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
    }),
  );

  readonly selectedDayEvents = computed(() =>
    this.sortEvents(
      this.monthEvents().filter((event) =>
        this.isSameDay(new Date(event.eventDate), this.selectedDate()),
      ),
    ),
  );

  createEventForm: CreateEventForm = {
    title: '',
    description: '',
    eventDate: this.toDateInput(new Date()),
    eventTime: '',
  };

  readonly calendarDays = computed((): CalendarDay[] => {
    const d = this.currentDate();
    const year = d.getFullYear();
    const month = d.getMonth();
    const today = new Date();

    const firstDay = new Date(year, month, 1);
    // Monday-based: getDay() returns 0=Sun,1=Mon... we need 0=Mon
    let startDow = firstDay.getDay(); // 0=Sun
    startDow = startDow === 0 ? 6 : startDow - 1; // convert to Mon=0

    const days: CalendarDay[] = [];

    // Previous month days
    for (let i = startDow - 1; i >= 0; i--) {
      const date = new Date(year, month, -i);
      days.push({
        date,
        isCurrentMonth: false,
        isToday: false,
        events: [],
      });
    }

    // Current month days
    const lastDay = new Date(year, month + 1, 0).getDate();
    for (let i = 1; i <= lastDay; i++) {
      const date = new Date(year, month, i);
      const isToday =
        date.getDate() === today.getDate() &&
        date.getMonth() === today.getMonth() &&
        date.getFullYear() === today.getFullYear();

      const events = this.monthEvents().filter((e) => {
        const ed = new Date(e.eventDate);
        return ed.getDate() === i && ed.getMonth() === month && ed.getFullYear() === year;
      });

      days.push({ date, isCurrentMonth: true, isToday, events });
    }

    // Fill remaining cells to complete 6 rows (42 cells)
    const remaining = 42 - days.length;
    for (let i = 1; i <= remaining; i++) {
      const date = new Date(year, month + 1, i);
      days.push({
        date,
        isCurrentMonth: false,
        isToday: false,
        events: [],
      });
    }

    return days;
  });

  ngOnInit(): void {
    this.syncCreateFormDate(this.selectedDate());
    this.loadMonthEvents();
    this.loadUpcomingEvents();
  }

  private readonly refreshEffect = effect(() => {
    const tick = this.calendarService.refreshTick();
    if (tick === 0) return;
    this.loadMonthEvents();
    this.loadUpcomingEvents();
  });

  private loadMonthEvents(): void {
    const d = this.currentDate();
    this.loading.set(true);
    this.error.set(null);
    this.calendarService.getMonthEvents(d.getFullYear(), d.getMonth() + 1).subscribe({
      next: (events) => {
        this.monthEvents.set(this.sortEvents(events));
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loading.set(false);
      },
    });
  }

  private loadUpcomingEvents(): void {
    this.calendarService.getUpcomingEvents(10).subscribe({
      next: (events) => this.upcomingEvents.set(events),
    });
  }

  prevMonth(): void {
    const d = this.currentDate();
    const next = new Date(d.getFullYear(), d.getMonth() - 1, 1);
    this.currentDate.set(next);
    this.selectedDate.set(this.startOfDay(next));
    this.syncCreateFormDate(next);
    this.loadMonthEvents();
  }

  nextMonth(): void {
    const d = this.currentDate();
    const next = new Date(d.getFullYear(), d.getMonth() + 1, 1);
    this.currentDate.set(next);
    this.selectedDate.set(this.startOfDay(next));
    this.syncCreateFormDate(next);
    this.loadMonthEvents();
  }

  selectDay(day: CalendarDay): void {
    const next = this.startOfDay(day.date);
    this.selectedDate.set(next);
    this.syncCreateFormDate(next);

    if (!day.isCurrentMonth) {
      this.currentDate.set(new Date(next.getFullYear(), next.getMonth(), 1));
      this.loadMonthEvents();
    }
  }

  focusEvent(event: CalendarEventDto): void {
    const eventDate = this.startOfDay(new Date(event.eventDate));
    const visibleMonth = this.currentDate();

    this.selectedDate.set(eventDate);
    this.syncCreateFormDate(eventDate);

    if (
      eventDate.getFullYear() !== visibleMonth.getFullYear()
      || eventDate.getMonth() !== visibleMonth.getMonth()
    ) {
      this.currentDate.set(new Date(eventDate.getFullYear(), eventDate.getMonth(), 1));
      this.loadMonthEvents();
    }
  }

  openEvent(event: CalendarEventDto, domEvent?: Event): void {
    domEvent?.stopPropagation();
    const route = this.getEventRoute(event);
    if (route) {
      this.router.navigateByUrl(route);
    }
  }

  hasEventRoute(event: CalendarEventDto): boolean {
    return this.getEventRoute(event) !== null;
  }

  createCustomEvent(): void {
    if (!this.isTeacher()) {
      return;
    }

    const title = this.createEventForm.title.trim();
    if (!title || !this.createEventForm.eventDate) {
      this.actionError.set('Укажите название и дату события.');
      return;
    }

    this.creating.set(true);
    this.actionError.set(null);

    this.calendarService.createEvent({
      title,
      description: this.createEventForm.description.trim() || undefined,
      eventDate: `${this.createEventForm.eventDate}T00:00:00`,
      eventTime: this.createEventForm.eventTime || undefined,
      type: CalendarEventType.Custom,
    }).subscribe({
      next: (event) => {
        const eventDate = this.startOfDay(new Date(event.eventDate));
        this.currentDate.set(new Date(eventDate.getFullYear(), eventDate.getMonth(), 1));
        this.selectedDate.set(eventDate);
        this.resetCreateForm(eventDate);
        this.creating.set(false);
        this.loadMonthEvents();
        this.loadUpcomingEvents();
      },
      error: (err) => {
        this.actionError.set(parseApiError(err).message);
        this.creating.set(false);
      },
    });
  }

  deleteEvent(event: CalendarEventDto, domEvent: Event): void {
    domEvent.stopPropagation();
    if (!this.isCustomEvent(event)) {
      return;
    }

    this.deletingId.set(event.id);
    this.actionError.set(null);

    this.calendarService.deleteEvent(event.id).subscribe({
      next: () => {
        this.monthEvents.update((items) => items.filter((item) => item.id !== event.id));
        this.upcomingEvents.update((items) => items.filter((item) => item.id !== event.id));
        this.deletingId.set(null);
      },
      error: (err) => {
        this.actionError.set(parseApiError(err).message);
        this.deletingId.set(null);
      },
    });
  }

  getEventDotClass(type: CalendarEventType): string {
    switch (type) {
      case CalendarEventType.Deadline: return 'event-dot--red';
      case CalendarEventType.Lesson: return 'event-dot--green';
      case CalendarEventType.Quiz: return 'event-dot--yellow';
      case CalendarEventType.Workshop: return 'event-dot--blue';
      case CalendarEventType.Custom: return 'event-dot--gray';
      default: return 'event-dot--gray';
    }
  }

  getTypeBadgeVariant(type: CalendarEventType): 'primary' | 'success' | 'warning' | 'danger' | 'neutral' {
    switch (type) {
      case CalendarEventType.Deadline: return 'danger';
      case CalendarEventType.Lesson: return 'success';
      case CalendarEventType.Quiz: return 'warning';
      case CalendarEventType.Workshop: return 'primary';
      case CalendarEventType.Custom: return 'neutral';
      default: return 'neutral';
    }
  }

  getTypeLabel(type: CalendarEventType): string {
    switch (type) {
      case CalendarEventType.Deadline: return 'Дедлайн';
      case CalendarEventType.Lesson: return 'Урок';
      case CalendarEventType.Quiz: return 'Тест';
      case CalendarEventType.Workshop: return 'Воркшоп';
      case CalendarEventType.Custom: return 'Другое';
      default: return type;
    }
  }

  getStatusClass(status?: DeadlineStatus): string {
    switch (status) {
      case DeadlineStatus.Completed: return 'event--done';
      case DeadlineStatus.InProgress: return 'event--in-progress';
      case DeadlineStatus.Pending: return 'event--pending';
      default: return '';
    }
  }

  getStatusLabel(status?: DeadlineStatus): string {
    switch (status) {
      case DeadlineStatus.Completed: return 'Сдано';
      case DeadlineStatus.InProgress: return 'В процессе';
      case DeadlineStatus.Pending: return 'Не начато';
      default: return '';
    }
  }

  formatEventDate(dateStr: string, timeStr?: string): string {
    const date = new Date(dateStr);
    const formatted = date.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' });
    return timeStr ? `${formatted}, ${timeStr}` : formatted;
  }

  formatEventTime(timeStr?: string): string {
    return timeStr || 'Весь день';
  }

  formatEventCount(count: number): string {
    const mod10 = count % 10;
    const mod100 = count % 100;

    if (mod10 === 1 && mod100 !== 11) return `${count} событие`;
    if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return `${count} события`;
    return `${count} событий`;
  }

  isSelectedDay(day: CalendarDay): boolean {
    return this.isSameDay(day.date, this.selectedDate());
  }

  isCustomEvent(event: CalendarEventDto): boolean {
    return event.type === CalendarEventType.Custom && !event.sourceType && !event.sourceId;
  }

  private getEventRoute(event: CalendarEventDto): string | null {
    const role = this.userRole();
    if (!role) {
      return null;
    }

    if (event.sourceType === 'Assignment' && event.sourceId) {
      return role === UserRole.Teacher
        ? `/teacher/assignment/${event.sourceId}/edit`
        : `/student/assignment/${event.sourceId}`;
    }

    if (event.sourceType === 'Test' && event.sourceId) {
      return role === UserRole.Teacher
        ? `/teacher/test/${event.sourceId}/edit`
        : `/student/test/${event.sourceId}/play`;
    }

    if (event.sourceType === 'ScheduleSlot') {
      return role === UserRole.Teacher ? '/teacher/schedule' : '/student/schedule';
    }

    if (event.courseId) {
      return role === UserRole.Teacher
        ? `/teacher/courses/${event.courseId}/editor`
        : `/student/course/${event.courseId}`;
    }

    return null;
  }

  private resetCreateForm(date: Date): void {
    this.createEventForm = {
      title: '',
      description: '',
      eventDate: this.toDateInput(date),
      eventTime: '',
    };
  }

  private syncCreateFormDate(date: Date): void {
    this.createEventForm.eventDate = this.toDateInput(date);
  }

  private sortEvents(events: CalendarEventDto[]): CalendarEventDto[] {
    return [...events].sort((left, right) => {
      const dateDiff = new Date(left.eventDate).getTime() - new Date(right.eventDate).getTime();
      if (dateDiff !== 0) {
        return dateDiff;
      }

      const leftTime = left.eventTime ?? '23:59';
      const rightTime = right.eventTime ?? '23:59';
      return leftTime.localeCompare(rightTime);
    });
  }

  private startOfDay(date: Date): Date {
    return new Date(date.getFullYear(), date.getMonth(), date.getDate());
  }

  private toDateInput(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private isSameDay(left: Date, right: Date): boolean {
    return left.getDate() === right.getDate()
      && left.getMonth() === right.getMonth()
      && left.getFullYear() === right.getFullYear();
  }
}

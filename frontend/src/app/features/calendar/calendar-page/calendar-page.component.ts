import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, ChevronLeft, ChevronRight, Calendar } from 'lucide-angular';
import { CalendarService } from '../services/calendar.service';
import { CalendarEventDto, CalendarEventType } from '../models/calendar.model';
import { parseApiError } from '../../../core/models/api-error.model';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';

interface CalendarDay {
  date: Date;
  isCurrentMonth: boolean;
  isToday: boolean;
  events: CalendarEventDto[];
}

@Component({
  selector: 'app-calendar-page',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, BadgeComponent],
  templateUrl: './calendar-page.component.html',
  styleUrl: './calendar-page.component.scss',
})
export class CalendarPageComponent implements OnInit {
  private readonly calendarService = inject(CalendarService);

  readonly icons = { ChevronLeft, ChevronRight, Calendar };

  readonly currentDate = signal(new Date());
  readonly monthEvents = signal<CalendarEventDto[]>([]);
  readonly upcomingEvents = signal<CalendarEventDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly CalendarEventType = CalendarEventType;

  readonly monthNames = [
    'Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
    'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь',
  ];

  readonly weekDays = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'];

  readonly currentMonthLabel = computed(() => {
    const d = this.currentDate();
    return `${this.monthNames[d.getMonth()]} ${d.getFullYear()}`;
  });

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
    this.loadMonthEvents();
    this.loadUpcomingEvents();
  }

  private loadMonthEvents(): void {
    const d = this.currentDate();
    this.loading.set(true);
    this.calendarService.getMonthEvents(d.getFullYear(), d.getMonth() + 1).subscribe({
      next: (events) => {
        this.monthEvents.set(events);
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
    this.currentDate.set(new Date(d.getFullYear(), d.getMonth() - 1, 1));
    this.loadMonthEvents();
  }

  nextMonth(): void {
    const d = this.currentDate();
    this.currentDate.set(new Date(d.getFullYear(), d.getMonth() + 1, 1));
    this.loadMonthEvents();
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

  formatEventDate(dateStr: string, timeStr?: string): string {
    const date = new Date(dateStr);
    const formatted = date.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' });
    return timeStr ? `${formatted}, ${timeStr}` : formatted;
  }
}

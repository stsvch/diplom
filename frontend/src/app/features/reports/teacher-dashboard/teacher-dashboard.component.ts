import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ArrowRight,
  BadgeDollarSign,
  BookCheck,
  BookOpen,
  CalendarDays,
  CheckCircle2,
  GraduationCap,
  LucideAngularModule,
  Sparkles,
  Target,
  Users,
} from 'lucide-angular';
import { parseApiError } from '../../../core/models/api-error.model';
import { StatsCardComponent } from '../../../shared/components/stats-card/stats-card.component';
import {
  TeacherDashboardCourseDto,
  TeacherDashboardDto,
  TeacherDashboardReviewItemDto,
  TeacherDashboardSessionDto,
} from '../models/reports.model';
import { ReportsService } from '../services/reports.service';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    LucideAngularModule,
    StatsCardComponent,
  ],
  templateUrl: './teacher-dashboard.component.html',
  styleUrl: './teacher-dashboard.component.scss',
})
export class TeacherDashboardComponent implements OnInit {
  private readonly reports = inject(ReportsService);

  readonly BookOpenIcon = BookOpen;
  readonly CheckCircleIcon = CheckCircle2;
  readonly UsersIcon = Users;
  readonly ReviewIcon = BookCheck;
  readonly TargetIcon = Target;
  readonly GradeIcon = GraduationCap;
  readonly EarningsIcon = BadgeDollarSign;
  readonly SessionsIcon = CalendarDays;
  readonly ArrowRightIcon = ArrowRight;
  readonly ActivityIcon = Sparkles;

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly dashboard = signal<TeacherDashboardDto | null>(null);

  readonly summary = computed(() => this.dashboard()?.summary ?? null);
  readonly earnings = computed(() => this.dashboard()?.earnings ?? null);
  readonly courses = computed(() => this.dashboard()?.courses ?? []);
  readonly pendingReviews = computed(() => this.dashboard()?.pendingReviews ?? []);
  readonly upcomingSessions = computed(() => this.dashboard()?.upcomingSessions ?? []);

  private readonly shortDateFormatter = new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: 'short',
  });

  private readonly dateTimeFormatter = new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: 'long',
    hour: '2-digit',
    minute: '2-digit',
  });

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading.set(true);
    this.error.set(null);

    this.reports.getTeacherDashboard().subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loading.set(false);
      },
    });
  }

  formatPercent(value: number): string {
    return `${value.toFixed(1).replace(/\.0$/, '')}%`;
  }

  formatMoney(value: number, currency: string): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: currency.toUpperCase(),
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(value);
  }

  formatShortDate(value: string): string {
    return this.shortDateFormatter.format(new Date(value));
  }

  formatDateTime(value: string): string {
    return this.dateTimeFormatter.format(new Date(value));
  }

  formatSessionRange(session: TeacherDashboardSessionDto): string {
    const start = new Date(session.startTime);
    const end = new Date(session.endTime);
    return `${this.dateTimeFormatter.format(start)} • ${end.toLocaleTimeString('ru-RU', {
      hour: '2-digit',
      minute: '2-digit',
    })}`;
  }

  getCourseStateLabel(course: TeacherDashboardCourseDto): string {
    if (course.isArchived) return 'Архив';
    if (course.isPublished) return 'Опубликован';
    return 'Черновик';
  }

  getCourseStateClass(course: TeacherDashboardCourseDto): string {
    if (course.isArchived) return 'pill pill--neutral';
    if (course.isPublished) return 'pill pill--success';
    return 'pill pill--warning';
  }

  getReviewKindLabel(item: TeacherDashboardReviewItemDto): string {
    return item.kind === 'Assignment' ? 'Задание' : 'Тест';
  }

  getReviewRoute(item: TeacherDashboardReviewItemDto): (string | null)[] {
    if (item.kind === 'Test') {
      return ['/teacher/test', item.sourceId, 'submissions'];
    }

    return ['/teacher/assignments'];
  }

  getSessionStatusLabel(status: string): string {
    switch (status) {
      case 'Booked':
        return 'Есть записи';
      case 'Completed':
        return 'Завершено';
      case 'Cancelled':
        return 'Отменено';
      default:
        return 'Свободно';
    }
  }

  getSessionStatusClass(status: string): string {
    switch (status) {
      case 'Booked':
        return 'pill pill--warning';
      case 'Completed':
        return 'pill pill--success';
      case 'Cancelled':
        return 'pill pill--neutral';
      default:
        return 'pill';
    }
  }
}

import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ArrowRight,
  BookOpen,
  CalendarClock,
  CheckCircle2,
  ClipboardList,
  GraduationCap,
  LucideAngularModule,
  Sparkles,
  Target,
} from 'lucide-angular';
import { parseApiError } from '../../../core/models/api-error.model';
import { StatsCardComponent } from '../../../shared/components/stats-card/stats-card.component';
import {
  StudentDashboardCourseDto,
  StudentDashboardDto,
  StudentDashboardGradeDto,
  StudentDashboardUpcomingItemDto,
} from '../models/reports.model';
import { ReportsService } from '../services/reports.service';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    LucideAngularModule,
    StatsCardComponent,
  ],
  templateUrl: './student-dashboard.component.html',
  styleUrl: './student-dashboard.component.scss',
})
export class StudentDashboardComponent implements OnInit {
  private readonly reports = inject(ReportsService);

  readonly BookOpenIcon = BookOpen;
  readonly TargetIcon = Target;
  readonly CheckCircleIcon = CheckCircle2;
  readonly GradeIcon = GraduationCap;
  readonly CalendarIcon = CalendarClock;
  readonly ActivityIcon = Sparkles;
  readonly ArrowRightIcon = ArrowRight;
  readonly AssignmentsIcon = ClipboardList;

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly dashboard = signal<StudentDashboardDto | null>(null);

  readonly summary = computed(() => this.dashboard()?.summary ?? null);
  readonly courses = computed(() => this.dashboard()?.courses ?? []);
  readonly recentGrades = computed(() => this.dashboard()?.recentGrades ?? []);
  readonly upcoming = computed(() => this.dashboard()?.upcoming ?? []);

  private readonly shortDateFormatter = new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: 'short',
  });

  private readonly longDateFormatter = new Intl.DateTimeFormat('ru-RU', {
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

    this.reports.getStudentDashboard().subscribe({
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

  formatLessons(course: StudentDashboardCourseDto): string {
    return `${course.completedLessons}/${course.totalLessons}`;
  }

  formatGrade(grade: StudentDashboardGradeDto): string {
    return `${grade.score}/${grade.maxScore}`;
  }

  formatShortDate(value: string): string {
    return this.shortDateFormatter.format(new Date(value));
  }

  formatEventDate(item: StudentDashboardUpcomingItemDto): string {
    const date = new Date(item.eventDate);

    if (item.eventTime) {
      const [hours, minutes] = item.eventTime.split(':');
      date.setHours(Number(hours), Number(minutes));
      return this.longDateFormatter.format(date);
    }

    return new Intl.DateTimeFormat('ru-RU', {
      day: '2-digit',
      month: 'long',
    }).format(date);
  }

  getEventKindLabel(item: StudentDashboardUpcomingItemDto): string {
    if (item.sourceType === 'Assignment') return 'Задание';
    if (item.sourceType === 'Test') return 'Тест';
    if (item.type === 'Lesson') return 'Занятие';
    if (item.type === 'Deadline') return 'Дедлайн';
    return 'Событие';
  }

  getStatusLabel(status?: string | null): string {
    switch (status) {
      case 'Completed':
        return 'Закрыто';
      case 'InProgress':
        return 'В работе';
      case 'Pending':
        return 'Ожидает';
      default:
        return 'Скоро';
    }
  }

  getStatusClass(status?: string | null): string {
    switch (status) {
      case 'Completed':
        return 'pill pill--success';
      case 'InProgress':
        return 'pill pill--warning';
      case 'Pending':
        return 'pill pill--neutral';
      default:
        return 'pill';
    }
  }

  getGradeClass(grade: StudentDashboardGradeDto): string {
    if (grade.percent >= 90) return 'score score--great';
    if (grade.percent >= 75) return 'score score--good';
    if (grade.percent >= 60) return 'score score--ok';
    return 'score score--bad';
  }

  getUpcomingRoute(item: StudentDashboardUpcomingItemDto): (string | null)[] {
    if (item.sourceType === 'Assignment' && item.sourceId) {
      return ['/student/assignment', item.sourceId];
    }

    if (item.sourceType === 'Test' && item.sourceId) {
      return ['/student/test', item.sourceId, 'play'];
    }

    if (item.courseId) {
      return ['/student/course', item.courseId];
    }

    return ['/student/calendar'];
  }
}

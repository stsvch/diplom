import { CommonModule } from '@angular/common';
import {
  Component,
  computed,
  effect,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  AlertTriangle,
  ArrowRight,
  BookOpen,
  ClipboardCheck,
  GraduationCap,
  LucideAngularModule,
  Target,
  TimerReset,
  TriangleAlert,
  Users,
} from 'lucide-angular';
import { parseApiError } from '../../../core/models/api-error.model';
import { StatsCardComponent } from '../../../shared/components/stats-card/stats-card.component';
import {
  TeacherCourseDeadlineItemDto,
  TeacherCourseGradeBucketDto,
  TeacherCourseReportDto,
  TeacherCourseRiskStudentDto,
  TeacherDashboardCourseDto,
  TeacherDashboardDto,
} from '../models/reports.model';
import { ReportsService } from '../services/reports.service';

@Component({
  selector: 'app-teacher-course-reports',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    LucideAngularModule,
    StatsCardComponent,
  ],
  templateUrl: './teacher-course-reports.component.html',
  styleUrl: './teacher-course-reports.component.scss',
})
export class TeacherCourseReportsComponent implements OnInit {
  private readonly reports = inject(ReportsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private reportRequestVersion = 0;

  readonly BookOpenIcon = BookOpen;
  readonly StudentsIcon = Users;
  readonly ProgressIcon = Target;
  readonly GradeIcon = GraduationCap;
  readonly ReviewIcon = ClipboardCheck;
  readonly OverdueIcon = AlertTriangle;
  readonly UpcomingIcon = TimerReset;
  readonly ArrowRightIcon = ArrowRight;
  readonly RiskIcon = TriangleAlert;

  readonly dashboardLoading = signal(false);
  readonly reportLoading = signal(false);
  readonly error = signal<string | null>(null);
  readonly dashboard = signal<TeacherDashboardDto | null>(null);
  readonly report = signal<TeacherCourseReportDto | null>(null);
  readonly selectedCourseId = signal<string | null>(null);
  readonly routeCourseId = signal<string | null>(null);

  readonly courses = computed(() => this.dashboard()?.courses ?? []);
  readonly summary = computed(() => this.report()?.summary ?? null);
  readonly gradeDistribution = computed(() => this.report()?.gradeDistribution ?? []);
  readonly atRiskStudents = computed(() => this.report()?.atRiskStudents ?? []);
  readonly deadlines = computed(() => this.report()?.deadlines ?? []);
  readonly hasCourses = computed(() => this.courses().length > 0);
  readonly hasGradeDistribution = computed(() =>
    this.gradeDistribution().some((bucket) => bucket.count > 0),
  );

  private readonly dateTimeFormatter = new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: 'long',
    hour: '2-digit',
    minute: '2-digit',
  });

  constructor() {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed())
      .subscribe((params) => this.routeCourseId.set(params.get('courseId')));

    effect(() => {
      const dashboard = this.dashboard();
      const routeCourseId = this.routeCourseId();

      if (!dashboard) {
        return;
      }

      const resolvedCourseId = this.resolveCourseId(dashboard.courses, routeCourseId);
      if (!resolvedCourseId) {
        this.selectedCourseId.set(null);
        this.report.set(null);
        return;
      }

      if (resolvedCourseId === this.selectedCourseId()) {
        return;
      }

      this.selectedCourseId.set(resolvedCourseId);
      this.loadCourseReport(resolvedCourseId);
    });
  }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.dashboardLoading.set(true);
    this.error.set(null);

    this.reports.getTeacherDashboard().subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.dashboardLoading.set(false);
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.dashboardLoading.set(false);
      },
    });
  }

  onCourseChange(courseId: string): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { courseId },
      queryParamsHandling: 'merge',
    });
  }

  retry(): void {
    if (!this.dashboard()) {
      this.loadDashboard();
      return;
    }

    if (this.selectedCourseId()) {
      this.loadCourseReport(this.selectedCourseId()!);
    }
  }

  formatPercent(value: number): string {
    return `${value.toFixed(1).replace(/\.0$/, '')}%`;
  }

  formatDeadline(value: string): string {
    return this.dateTimeFormatter.format(new Date(value));
  }

  formatStudentProgress(student: TeacherCourseRiskStudentDto): string {
    return `${student.completedLessons}/${student.totalLessons}`;
  }

  getCourseStateLabel(course: TeacherDashboardCourseDto): string {
    if (course.isArchived) return 'Архив';
    if (course.isPublished) return 'Опубликован';
    return 'Черновик';
  }

  getCourseStateClass(summary: TeacherCourseReportDto['summary']): string {
    if (summary.isArchived) return 'pill pill--neutral';
    if (summary.isPublished) return 'pill pill--success';
    return 'pill pill--warning';
  }

  getGradeBucketStyle(bucket: TeacherCourseGradeBucketDto): { width: string } {
    return { width: `${bucket.sharePercent}%` };
  }

  getRiskClass(student: TeacherCourseRiskStudentDto): string {
    const overdue = student.overdueAssignmentsCount + student.overdueTestsCount;
    if (overdue >= 3 || student.progressPercent < 35) return 'risk-pill risk-pill--high';
    if (overdue > 0 || student.progressPercent < 60) return 'risk-pill risk-pill--medium';
    return 'risk-pill risk-pill--low';
  }

  getRiskLabel(student: TeacherCourseRiskStudentDto): string {
    const overdue = student.overdueAssignmentsCount + student.overdueTestsCount;
    if (overdue >= 3 || student.progressPercent < 35) return 'Высокий риск';
    if (overdue > 0 || student.progressPercent < 60) return 'Нужен фокус';
    return 'Наблюдение';
  }

  getDeadlineKindLabel(item: TeacherCourseDeadlineItemDto): string {
    return item.kind === 'Assignment' ? 'Задание' : 'Тест';
  }

  getDeadlineClass(item: TeacherCourseDeadlineItemDto): string {
    return item.isOverdue ? 'pill pill--danger' : 'pill pill--warning';
  }

  getDeadlineStatus(item: TeacherCourseDeadlineItemDto): string {
    return item.isOverdue ? 'Просрочено' : 'Скоро';
  }

  getDeadlineRoute(item: TeacherCourseDeadlineItemDto): (string | null)[] {
    if (item.kind === 'Test') {
      return ['/teacher/test', item.sourceId, 'submissions'];
    }

    return ['/teacher/assignments'];
  }

  private resolveCourseId(
    courses: TeacherDashboardCourseDto[],
    routeCourseId: string | null,
  ): string | null {
    if (!courses.length) {
      return null;
    }

    if (routeCourseId && courses.some((course) => course.courseId === routeCourseId)) {
      return routeCourseId;
    }

    return courses[0].courseId;
  }

  private loadCourseReport(courseId: string): void {
    const requestVersion = ++this.reportRequestVersion;

    this.reportLoading.set(true);
    this.error.set(null);

    this.reports.getTeacherCourseReport(courseId).subscribe({
      next: (report) => {
        if (requestVersion !== this.reportRequestVersion || this.selectedCourseId() !== courseId) {
          return;
        }

        this.report.set(report);
        this.reportLoading.set(false);
      },
      error: (err) => {
        if (requestVersion !== this.reportRequestVersion || this.selectedCourseId() !== courseId) {
          return;
        }

        this.error.set(parseApiError(err).message);
        this.reportLoading.set(false);
      },
    });
  }
}

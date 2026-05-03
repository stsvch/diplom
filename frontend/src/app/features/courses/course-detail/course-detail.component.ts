import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  LucideAngularModule,
  ChevronLeft,
  ChevronDown,
  ChevronUp,
  Star,
  Clock,
  Users,
  BookOpen,
  PlayCircle,
  FileText,
  Download,
  CheckCircle2,
  Lock,
  Tag,
  GraduationCap,
  Award,
} from 'lucide-angular';
import { CoursesService } from '../services/courses.service';
import { PreviewModeService } from '../services/preview-mode.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { CourseDetailDto, CourseModuleDetailDto } from '../models/course.model';
import { ProgressService } from '../../progress/services/progress.service';
import { AuthService } from '../../../core/services/auth.service';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { ProgressBarComponent } from '../../../shared/components/progress-bar/progress-bar.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { DurationPipe } from '../../../shared/pipes/duration.pipe';
import { PaymentsService } from '../../payments/services/payments.service';

@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [
    RouterLink,
    LucideAngularModule,
    BadgeComponent,
    ProgressBarComponent,
    ButtonComponent,
    AvatarComponent,
    DurationPipe,
  ],
  templateUrl: './course-detail.component.html',
  styleUrl: './course-detail.component.scss',
})
export class CourseDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly coursesService = inject(CoursesService);
  private readonly toastService = inject(ToastService);
  private readonly progressService = inject(ProgressService);
  private readonly authService = inject(AuthService);
  private readonly previewMode = inject(PreviewModeService);
  private readonly paymentsService = inject(PaymentsService);

  readonly isPreview = this.previewMode.isPreview;
  readonly actualProgress = signal<number | null>(null);

  readonly ChevronLeftIcon = ChevronLeft;
  readonly ChevronDownIcon = ChevronDown;
  readonly ChevronUpIcon = ChevronUp;
  readonly StarIcon = Star;
  readonly ClockIcon = Clock;
  readonly UsersIcon = Users;
  readonly BookOpenIcon = BookOpen;
  readonly PlayCircleIcon = PlayCircle;
  readonly FileTextIcon = FileText;
  readonly DownloadIcon = Download;
  readonly CheckCircleIcon = CheckCircle2;
  readonly LockIcon = Lock;
  readonly TagIcon = Tag;
  readonly GraduationCapIcon = GraduationCap;
  readonly AwardIcon = Award;

  readonly loading = signal(true);
  readonly enrolling = signal(false);
  readonly savePaymentMethod = signal(false);
  readonly course = signal<CourseDetailDto | null>(null);
  readonly expandedModules = signal<Set<string>>(new Set());

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.loadCourse(id);
  }

  loadCourse(id: string): void {
    this.loading.set(true);
    this.coursesService.getCourseById(id).subscribe({
      next: (data) => {
        this.course.set(data);
        this.loading.set(false);
        // expand first module by default
        if (data.modules.length > 0) {
          this.expandedModules.set(new Set([data.modules[0].id]));
        }
        // Load real progress only for students (not in preview)
        if (!this.isPreview() && this.authService.userRole() === 'Student' && data.progress !== undefined && data.progress !== null) {
          this.loadCourseProgress(id);
        }
      },
      error: (err: ApiError) => {
        this.loading.set(false);
        this.toastService.error(err.message);
      },
    });
  }

  loadCourseProgress(courseId: string): void {
    this.progressService.getCourseProgress(courseId).subscribe({
      next: (progress) => {
        this.actualProgress.set(progress.progressPercent);
      },
      error: () => {
        // silently ignore — falls back to course.progress
      },
    });
  }

  toggleModule(id: string): void {
    this.expandedModules.update((set) => {
      const next = new Set(set);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  isModuleExpanded(id: string): boolean {
    return this.expandedModules().has(id);
  }

  enroll(): void {
    if (this.isPreview()) {
      this.toastService.info('Запись недоступна в режиме предпросмотра');
      return;
    }

    if (!this.authService.isAuthenticated()) {
      this.toastService.info('Чтобы записаться на курс, сначала войдите или зарегистрируйтесь.');
      return;
    }

    if (!this.isStudentView()) {
      this.toastService.info('Покупка и запись на курс доступны только для аккаунта студента.');
      return;
    }

    const c = this.course();
    if (!c) return;
    this.enrolling.set(true);
    if (c.isFree) {
      this.coursesService.enrollCourse(c.id).subscribe({
        next: () => {
          this.enrolling.set(false);
          this.toastService.success('Вы успешно записались на курс!');
        },
        error: (err: ApiError) => {
          this.enrolling.set(false);
          this.toastService.error(err.message);
        },
      });
      return;
    }

    this.paymentsService.createCourseCheckout(c.id, this.savePaymentMethod()).subscribe({
      next: (session) => {
        this.enrolling.set(false);
        window.location.assign(session.checkoutUrl);
      },
      error: (err: ApiError) => {
        this.enrolling.set(false);
        this.toastService.error(err.message);
      },
    });
  }

  get levelLabel(): string {
    const map: Record<string, string> = {
      Beginner: 'Начальный',
      Intermediate: 'Средний',
      Advanced: 'Продвинутый',
    };
    return map[this.course()?.level ?? ''] ?? (this.course()?.level ?? '');
  }

  get levelVariant(): 'primary' | 'success' | 'warning' | 'danger' | 'neutral' {
    const map: Record<string, 'primary' | 'success' | 'warning' | 'danger' | 'neutral'> = {
      Beginner: 'success',
      Intermediate: 'warning',
      Advanced: 'danger',
    };
    return map[this.course()?.level ?? ''] ?? 'neutral';
  }

  get tags(): string[] {
    return (this.course()?.tags ?? '')
      .split(',')
      .map((t) => t.trim())
      .filter(Boolean);
  }

  get totalLessons(): number {
    return this.course()?.modules.reduce((sum, m) => sum + m.lessons.length, 0) ?? 0;
  }

  isEnrolled(): boolean {
    return (this.course()?.progress ?? undefined) !== undefined;
  }

  isGuestView(): boolean {
    return !this.authService.isAuthenticated();
  }

  isStudentView(): boolean {
    return this.authService.userRole() === 'Student';
  }

  isReadOnlyAuthenticatedView(): boolean {
    return this.authService.isAuthenticated() && !this.isStudentView();
  }

  get catalogLink(): string {
    return this.isStudentView() ? '/student/catalog' : '/catalog';
  }

  get dashboardLink(): string {
    switch (this.authService.userRole()) {
      case 'Teacher':
        return '/teacher/dashboard';
      case 'Admin':
        return '/admin/dashboard';
      case 'Student':
        return '/student/dashboard';
      default:
        return '/';
    }
  }

  get dashboardLabel(): string {
    switch (this.authService.userRole()) {
      case 'Teacher':
        return 'Открыть кабинет преподавателя';
      case 'Admin':
        return 'Открыть админ-панель';
      case 'Student':
        return 'Открыть дашборд';
      default:
        return 'На главную';
    }
  }

  getLessonIcon(lesson: { blocksCount: number }): any {
    return lesson.blocksCount > 0 ? this.PlayCircleIcon : this.FileTextIcon;
  }
}

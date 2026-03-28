import {
  Component,
  inject,
  signal,
  OnInit,
  computed,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  LucideAngularModule,
  ChevronLeft,
  Eye,
  Loader2,
  Users,
  BarChart2,
  CheckCircle2,
  Edit3,
} from 'lucide-angular';
import { TestsService } from '../services/tests.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';
import { TestAttemptDto, TestDetailDto } from '../models/test.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';

@Component({
  selector: 'app-test-submissions',
  standalone: true,
  imports: [
    RouterLink,
    LucideAngularModule,
    ButtonComponent,
    BadgeComponent,
  ],
  templateUrl: './test-submissions.component.html',
  styleUrl: './test-submissions.component.scss',
})
export class TestSubmissionsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly testsService = inject(TestsService);
  private readonly toastService = inject(ToastService);

  readonly ChevronLeftIcon = ChevronLeft;
  readonly EyeIcon = Eye;
  readonly Loader2Icon = Loader2;
  readonly UsersIcon = Users;
  readonly ChartIcon = BarChart2;
  readonly CheckIcon = CheckCircle2;
  readonly EditIcon = Edit3;

  readonly loading = signal(true);
  readonly test = signal<TestDetailDto | null>(null);
  readonly submissions = signal<TestAttemptDto[]>([]);

  testId = '';

  readonly totalAttempts = computed(() => this.submissions().length);

  readonly averageScore = computed(() => {
    const completed = this.submissions().filter((s) => s.score !== undefined);
    if (!completed.length) return 0;
    const sum = completed.reduce((acc, s) => acc + (s.score ?? 0), 0);
    return Math.round(sum / completed.length);
  });

  readonly passRate = computed(() => {
    const completed = this.submissions().filter((s) => s.score !== undefined && s.maxScore > 0);
    if (!completed.length) return 0;
    const passed = completed.filter((s) => ((s.score ?? 0) / s.maxScore) * 100 >= 60);
    return Math.round((passed.length / completed.length) * 100);
  });

  ngOnInit(): void {
    this.testId = this.route.snapshot.paramMap.get('testId') ?? '';
    if (this.testId) {
      this.loadData();
    }
  }

  loadData(): void {
    this.loading.set(true);
    this.testsService.getTest(this.testId).subscribe({
      next: (test) => {
        this.test.set(test);
        this.testsService.getSubmissions(this.testId).subscribe({
          next: (subs) => {
            this.submissions.set(subs.sort((a, b) =>
              new Date(b.startedAt).getTime() - new Date(a.startedAt).getTime()
            ));
            this.loading.set(false);
          },
          error: (err) => {
            this.loading.set(false);
            this.toastService.error(parseApiError(err).message);
          },
        });
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  viewAttempt(attemptId: string): void {
    this.router.navigate(['/teacher/test', this.testId, 'grade', attemptId]);
  }

  getStatusVariant(status: string): 'primary' | 'success' | 'warning' | 'danger' | 'neutral' {
    switch (status) {
      case 'Completed': return 'success';
      case 'InProgress': return 'primary';
      case 'NeedsReview': return 'warning';
      case 'Graded': return 'neutral';
      default: return 'neutral';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Completed': return 'Завершён';
      case 'InProgress': return 'В процессе';
      case 'NeedsReview': return 'На проверке';
      case 'Graded': return 'Оценён';
      default: return status;
    }
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  get backUrl(): string {
    return '/teacher/courses';
  }
}

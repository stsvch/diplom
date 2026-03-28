import {
  Component,
  inject,
  signal,
  OnInit,
  computed,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  LucideAngularModule,
  ChevronLeft,
  CheckCircle2,
  XCircle,
  Clock,
  RefreshCw,
  BookOpen,
  Loader2,
  MessageSquare,
} from 'lucide-angular';
import { TestsService } from '../services/tests.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';
import { TestAttemptDetailDto, QuestionDto } from '../models/test.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { ProgressBarComponent } from '../../../shared/components/progress-bar/progress-bar.component';

@Component({
  selector: 'app-test-result',
  standalone: true,
  imports: [
    LucideAngularModule,
    ButtonComponent,
    BadgeComponent,
    ProgressBarComponent,
  ],
  templateUrl: './test-result.component.html',
  styleUrl: './test-result.component.scss',
})
export class TestResultComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly testsService = inject(TestsService);
  private readonly toastService = inject(ToastService);

  readonly ChevronLeftIcon = ChevronLeft;
  readonly CheckCircleIcon = CheckCircle2;
  readonly XCircleIcon = XCircle;
  readonly ClockIcon = Clock;
  readonly RefreshIcon = RefreshCw;
  readonly BookOpenIcon = BookOpen;
  readonly Loader2Icon = Loader2;
  readonly MessageIcon = MessageSquare;

  readonly loading = signal(true);
  readonly attempt = signal<TestAttemptDetailDto | null>(null);
  readonly myAttempts = signal<{ id: string; attemptNumber: number; status: string }[]>([]);

  testId = '';
  attemptId = '';
  maxAttempts: number | null = null;

  readonly scorePercent = computed(() => {
    const a = this.attempt();
    if (!a || !a.maxScore) return 0;
    return Math.round(((a.score ?? 0) / a.maxScore) * 100);
  });

  readonly isPassed = computed(() => this.scorePercent() >= 60);

  readonly timeTaken = computed(() => {
    const a = this.attempt();
    if (!a?.completedAt) return null;
    const start = new Date(a.startedAt).getTime();
    const end = new Date(a.completedAt).getTime();
    const secs = Math.round((end - start) / 1000);
    const m = Math.floor(secs / 60);
    const s = secs % 60;
    return `${m} мин ${s} с`;
  });

  readonly correctCount = computed(() => {
    const a = this.attempt();
    if (!a) return 0;
    return a.responses.filter((r) => r.isCorrect).length;
  });

  readonly canRetry = computed(() => {
    if (this.maxAttempts === null) return true;
    return this.myAttempts().length < this.maxAttempts;
  });

  ngOnInit(): void {
    this.testId = this.route.snapshot.paramMap.get('testId') ?? '';
    this.attemptId = this.route.snapshot.paramMap.get('attemptId') ?? '';

    if (this.attemptId) {
      this.loadAttempt();
    }

    if (this.testId) {
      this.loadMyAttempts();
    }
  }

  loadAttempt(): void {
    this.loading.set(true);
    this.testsService.getAttempt(this.attemptId).subscribe({
      next: (data) => {
        this.attempt.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  loadMyAttempts(): void {
    this.testsService.getMyAttempts(this.testId).subscribe({
      next: (data) => {
        this.myAttempts.set(data);
      },
      error: () => {},
    });
  }

  getQuestion(questionId: string): QuestionDto | undefined {
    return this.attempt()?.questions?.find((q) => q.id === questionId);
  }

  getOptionText(question: QuestionDto, optionId: string): string {
    const opt = question.answerOptions.find((o) => o.id === optionId);
    return opt?.text ?? optionId;
  }

  getCorrectOptions(question: QuestionDto): string {
    return question.answerOptions
      .filter((o) => o.isCorrect)
      .map((o) => o.text)
      .join(', ');
  }

  retryTest(): void {
    this.router.navigate(['/student/test', this.testId, 'play']);
  }

  backToLesson(): void {
    this.router.navigate(['/student/courses']);
  }
}

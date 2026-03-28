import {
  Component,
  inject,
  signal,
  OnInit,
  OnDestroy,
  computed,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  ChevronLeft,
  ChevronRight,
  Clock,
  AlertTriangle,
  CheckCircle2,
  Loader2,
} from 'lucide-angular';
import { TestsService } from '../services/tests.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';
import { StudentQuestionDto, StudentAnswerOptionDto } from '../models/test.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { ProgressBarComponent } from '../../../shared/components/progress-bar/progress-bar.component';

interface LocalAnswer {
  selectedOptionIds: string[];
  textAnswer: string;
}

@Component({
  selector: 'app-test-player',
  standalone: true,
  imports: [
    FormsModule,
    LucideAngularModule,
    ButtonComponent,
    ProgressBarComponent,
  ],
  templateUrl: './test-player.component.html',
  styleUrl: './test-player.component.scss',
})
export class TestPlayerComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly testsService = inject(TestsService);
  private readonly toastService = inject(ToastService);

  readonly ChevronLeftIcon = ChevronLeft;
  readonly ChevronRightIcon = ChevronRight;
  readonly ClockIcon = Clock;
  readonly AlertIcon = AlertTriangle;
  readonly CheckIcon = CheckCircle2;
  readonly Loader2Icon = Loader2;

  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly showConfirmDialog = signal(false);
  readonly savingAnswer = signal(false);

  testId = '';
  attemptId = '';

  readonly questions = signal<StudentQuestionDto[]>([]);
  readonly currentIndex = signal(0);
  readonly answers = signal<Map<string, LocalAnswer>>(new Map());
  readonly timeLimitMinutes = signal<number | null>(null);
  readonly remainingSeconds = signal(0);

  private timerInterval: ReturnType<typeof setInterval> | null = null;

  readonly currentQuestion = computed(() => {
    const qs = this.questions();
    const idx = this.currentIndex();
    return qs[idx] ?? null;
  });

  readonly progress = computed(() => {
    const qs = this.questions();
    if (!qs.length) return 0;
    return ((this.currentIndex() + 1) / qs.length) * 100;
  });

  readonly answeredCount = computed(() => {
    const qs = this.questions();
    const ans = this.answers();
    return qs.filter((q) => {
      const a = ans.get(q.id);
      if (!a) return false;
      if (q.type === 'TextInput' || q.type === 'OpenAnswer') return a.textAnswer.trim().length > 0;
      return a.selectedOptionIds.length > 0;
    }).length;
  });

  readonly formattedTimer = computed(() => {
    const secs = this.remainingSeconds();
    const m = Math.floor(secs / 60);
    const s = secs % 60;
    return `${m}:${s.toString().padStart(2, '0')}`;
  });

  readonly timerWarning = computed(() => {
    const limit = this.timeLimitMinutes();
    if (!limit) return false;
    return this.remainingSeconds() <= 60;
  });

  ngOnInit(): void {
    this.testId = this.route.snapshot.paramMap.get('testId') ?? '';
    if (!this.testId) return;
    this.startTest();
  }

  ngOnDestroy(): void {
    this.clearTimer();
  }

  startTest(): void {
    this.loading.set(true);
    this.testsService.startAttempt(this.testId).subscribe({
      next: (data) => {
        this.attemptId = data.attemptId;
        const sorted = [...data.questions].sort((a, b) => a.orderIndex - b.orderIndex);
        this.questions.set(sorted);

        const initialAnswers = new Map<string, LocalAnswer>();
        sorted.forEach((q) => {
          initialAnswers.set(q.id, { selectedOptionIds: [], textAnswer: '' });
        });
        this.answers.set(initialAnswers);

        if (data.timeLimitMinutes) {
          this.timeLimitMinutes.set(data.timeLimitMinutes);
          this.remainingSeconds.set(data.timeLimitMinutes * 60);
          this.startTimer();
        }

        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error(parseApiError(err).message);
        this.router.navigate(['/student/courses']);
      },
    });
  }

  private startTimer(): void {
    this.timerInterval = setInterval(() => {
      const remaining = this.remainingSeconds();
      if (remaining <= 0) {
        this.clearTimer();
        this.toastService.error('Время вышло! Тест отправляется автоматически.');
        this.saveCurrentAnswer();
        this.doSubmit();
      } else {
        this.remainingSeconds.set(remaining - 1);
      }
    }, 1000);
  }

  private clearTimer(): void {
    if (this.timerInterval !== null) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }

  getAnswer(questionId: string): LocalAnswer {
    return this.answers().get(questionId) ?? { selectedOptionIds: [], textAnswer: '' };
  }

  isOptionSelected(questionId: string, optionId: string): boolean {
    return this.getAnswer(questionId).selectedOptionIds.includes(optionId);
  }

  setSingleAnswer(questionId: string, optionId: string): void {
    this.updateAnswer(questionId, { selectedOptionIds: [optionId], textAnswer: '' });
  }

  toggleMultiAnswer(questionId: string, optionId: string): void {
    const current = this.getAnswer(questionId).selectedOptionIds;
    const updated = current.includes(optionId)
      ? current.filter((id) => id !== optionId)
      : [...current, optionId];
    this.updateAnswer(questionId, { selectedOptionIds: updated, textAnswer: '' });
  }

  setTextAnswer(questionId: string, text: string): void {
    this.updateAnswer(questionId, { selectedOptionIds: [], textAnswer: text });
  }

  setMatchingAnswer(questionId: string, questionOptionId: string, selectedPairValue: string): void {
    // For matching: we store the selectedOptionIds as ["leftId:rightValue"] pairs
    const current = this.getAnswer(questionId).selectedOptionIds;
    const filtered = current.filter((v) => !v.startsWith(questionOptionId + ':'));
    const updated = selectedPairValue ? [...filtered, `${questionOptionId}:${selectedPairValue}`] : filtered;
    this.updateAnswer(questionId, { selectedOptionIds: updated, textAnswer: '' });
  }

  getMatchingSelection(questionId: string, leftOptionId: string): string {
    const ans = this.getAnswer(questionId).selectedOptionIds;
    const found = ans.find((v) => v.startsWith(leftOptionId + ':'));
    return found ? found.split(':').slice(1).join(':') : '';
  }

  private updateAnswer(questionId: string, answer: LocalAnswer): void {
    const map = new Map(this.answers());
    map.set(questionId, answer);
    this.answers.set(map);
  }

  private saveCurrentAnswer(): void {
    const q = this.currentQuestion();
    if (!q || !this.attemptId) return;

    const ans = this.getAnswer(q.id);
    let selectedOptionIds: string[] | undefined;
    let textAnswer: string | undefined;

    if (q.type === 'TextInput' || q.type === 'OpenAnswer') {
      textAnswer = ans.textAnswer;
    } else if (q.type === 'Matching') {
      // Send full "optionId:pairValue" pairs for backend grading
      selectedOptionIds = ans.selectedOptionIds;
    } else {
      selectedOptionIds = ans.selectedOptionIds;
    }

    this.savingAnswer.set(true);
    this.testsService
      .submitAnswer(this.attemptId, {
        questionId: q.id,
        selectedOptionIds,
        textAnswer,
      })
      .subscribe({
        next: () => this.savingAnswer.set(false),
        error: () => this.savingAnswer.set(false),
      });
  }

  goToQuestion(index: number): void {
    if (index < 0 || index >= this.questions().length) return;
    this.saveCurrentAnswer();
    this.currentIndex.set(index);
  }

  prevQuestion(): void {
    this.goToQuestion(this.currentIndex() - 1);
  }

  nextQuestion(): void {
    this.goToQuestion(this.currentIndex() + 1);
  }

  openConfirmDialog(): void {
    this.saveCurrentAnswer();
    this.showConfirmDialog.set(true);
  }

  closeConfirmDialog(): void {
    this.showConfirmDialog.set(false);
  }

  doSubmit(): void {
    this.clearTimer();
    this.showConfirmDialog.set(false);
    this.submitting.set(true);

    this.testsService.submitAttempt(this.attemptId).subscribe({
      next: () => {
        this.submitting.set(false);
        this.router.navigate(['/student/test', this.testId, 'result', this.attemptId]);
      },
      error: (err) => {
        this.submitting.set(false);
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  isAnswered(questionId: string): boolean {
    const a = this.answers().get(questionId);
    if (!a) return false;
    const q = this.questions().find((q) => q.id === questionId);
    if (!q) return false;
    if (q.type === 'TextInput' || q.type === 'OpenAnswer') return a.textAnswer.trim().length > 0;
    return a.selectedOptionIds.length > 0;
  }
}

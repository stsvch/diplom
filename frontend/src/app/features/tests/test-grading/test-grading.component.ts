import {
  Component,
  inject,
  signal,
  OnInit,
  computed,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  ChevronLeft,
  Save,
  Loader2,
  MessageSquare,
  User,
} from 'lucide-angular';
import { TestsService } from '../services/tests.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';
import { TestAttemptDetailDto, QuestionDto, TestResponseDto } from '../models/test.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';

interface GradeEntry {
  responseId: string;
  questionText: string;
  studentAnswer: string;
  maxPoints: number;
  points: number;
  comment: string;
}

@Component({
  selector: 'app-test-grading',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    LucideAngularModule,
    ButtonComponent,
    BadgeComponent,
  ],
  templateUrl: './test-grading.component.html',
  styleUrl: './test-grading.component.scss',
})
export class TestGradingComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly testsService = inject(TestsService);
  private readonly toastService = inject(ToastService);

  readonly ChevronLeftIcon = ChevronLeft;
  readonly SaveIcon = Save;
  readonly Loader2Icon = Loader2;
  readonly MessageIcon = MessageSquare;
  readonly UserIcon = User;

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly attempt = signal<TestAttemptDetailDto | null>(null);
  readonly gradeEntries = signal<GradeEntry[]>([]);

  testId = '';
  attemptId = '';

  ngOnInit(): void {
    this.testId = this.route.snapshot.paramMap.get('testId') ?? '';
    this.attemptId = this.route.snapshot.paramMap.get('attemptId') ?? '';
    if (this.attemptId) {
      this.loadAttempt();
    }
  }

  loadAttempt(): void {
    this.loading.set(true);
    this.testsService.getAttempt(this.attemptId).subscribe({
      next: (data) => {
        this.attempt.set(data);

        // Build grade entries for OpenAnswer questions
        const entries: GradeEntry[] = [];
        for (const response of data.responses ?? []) {
          const q = data.questions?.find((q) => q.id === response.questionId);
          if (!q || q.type !== 'OpenAnswer') continue;

          entries.push({
            responseId: response.id,
            questionText: q.text,
            studentAnswer: response.textAnswer ?? '',
            maxPoints: q.points,
            points: response.points ?? 0,
            comment: response.teacherComment ?? '',
          });
        }
        this.gradeEntries.set(entries);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  updatePoints(responseId: string, points: number): void {
    this.gradeEntries.update((entries) =>
      entries.map((e) => (e.responseId === responseId ? { ...e, points } : e)),
    );
  }

  updateComment(responseId: string, comment: string): void {
    this.gradeEntries.update((entries) =>
      entries.map((e) => (e.responseId === responseId ? { ...e, comment } : e)),
    );
  }

  saveGrades(): void {
    const entries = this.gradeEntries();
    if (!entries.length) {
      this.toastService.error('Нет вопросов для проверки');
      return;
    }

    this.saving.set(true);
    let pending = entries.length;

    entries.forEach((entry) => {
      this.testsService
        .gradeResponse(entry.responseId, {
          points: Math.min(entry.points, entry.maxPoints),
          comment: entry.comment || undefined,
        })
        .subscribe({
          next: () => {
            pending--;
            if (pending === 0) {
              this.saving.set(false);
              this.toastService.success('Оценки сохранены');
              this.router.navigate(['/teacher/test', this.testId, 'submissions']);
            }
          },
          error: (err) => {
            pending--;
            if (pending === 0) this.saving.set(false);
            this.toastService.error(parseApiError(err).message);
          },
        });
    });
  }

  get backUrl(): string {
    return `/teacher/test/${this.testId}/submissions`;
  }
}

import {
  Component,
  inject,
  signal,
  OnInit,
  OnDestroy,
  computed,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { LucideAngularModule } from 'lucide-angular';
import { BlockHostComponent, BlockViewerHostComponent } from '../../content/components';
import { ContentService, BlockAttemptsService } from '../../content/services';
import {
  LessonBlockDto,
  LessonBlockAnswer,
  LessonBlockAttemptDto,
  LessonProgressDto as ContentLessonProgressDto,
  SubmitAttemptResult,
} from '../../content/models';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { PreviewModeService } from '../services/preview-mode.service';
import { ProgressService } from '../../progress/services/progress.service';
import { LessonProgressDto as ManualLessonProgressDto } from '../../progress/models/progress.model';

interface BlockWithAttempt {
  block: LessonBlockDto;
  attempt: LessonBlockAttemptDto | null;
  lastResult: SubmitAttemptResult | null;
}

@Component({
  selector: 'app-lesson-view',
  standalone: true,
  imports: [LucideAngularModule, BlockHostComponent, BlockViewerHostComponent],
  templateUrl: './lesson-view.component.html',
  styleUrl: './lesson-view.component.scss',
})
export class LessonViewComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly contentService = inject(ContentService);
  private readonly attemptsService = inject(BlockAttemptsService);
  private readonly progressService = inject(ProgressService);
  private readonly toast = inject(ToastService);
  private readonly previewMode = inject(PreviewModeService);

  readonly isPreview = this.previewMode.isPreview;

  private readonly destroy$ = new Subject<void>();

  lessonId = signal('');
  loading = signal(true);
  entries = signal<BlockWithAttempt[]>([]);
  progress = signal<ContentLessonProgressDto | null>(null);
  manualProgress = signal<ManualLessonProgressDto | null>(null);

  isCompleted = computed(() =>
    (this.progress()?.isCompleted ?? false) || (this.manualProgress()?.isCompleted ?? false),
  );
  progressPct = computed(() => this.progress()?.percentage ?? 0);
  canToggleCompletion = computed(() =>
    !this.isPreview() && !(this.progress()?.isCompleted ?? false),
  );

  ngOnInit() {
    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
        const id = params.get('id');
        if (!id) return;
        this.lessonId.set(id);
        this.load();
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private load() {
    this.loading.set(true);
    this.contentService.getByLesson(this.lessonId()).subscribe({
      next: (blocks) => {
        if (blocks.length === 0) {
          this.entries.set([]);
          this.loading.set(false);
          return;
        }
        if (this.isPreview()) {
          this.entries.set(blocks.map((b) => ({ block: b, attempt: null, lastResult: null })));
          this.loading.set(false);
          return;
        }
        const attemptRequests = blocks.map((b) => this.attemptsService.getMyAttempt(b.id));
        forkJoin(attemptRequests).subscribe({
          next: (attempts) => {
            const combined = blocks.map((b, i) => ({
              block: b,
              attempt: attempts[i] ?? null,
              lastResult: null,
            }));
            this.entries.set(combined);
            this.loadProgress();
            this.loading.set(false);
          },
          error: () => {
            this.entries.set(blocks.map((b) => ({ block: b, attempt: null, lastResult: null })));
            this.loading.set(false);
          },
        });
      },
      error: (err) => {
        const e = err.error as ApiError | undefined;
        this.toast.error(e?.message ?? 'Не удалось загрузить урок');
        this.loading.set(false);
      },
    });
  }

  private loadProgress() {
    forkJoin({
      content: this.attemptsService.getMyLessonProgress(this.lessonId()),
      manual: this.progressService.getLessonProgress(this.lessonId()),
    }).subscribe({
      next: ({ content, manual }) => {
        this.progress.set(content);
        this.manualProgress.set(manual);
      },
      error: () => {
        this.progress.set(null);
        this.manualProgress.set(null);
      },
    });
  }

  toggleCompletion() {
    if (!this.canToggleCompletion()) {
      return;
    }

    const lessonId = this.lessonId();
    const isCompleted = this.manualProgress()?.isCompleted ?? false;

    if (isCompleted) {
      this.progressService.uncompleteLesson(lessonId).subscribe({
        next: () => {
          this.manualProgress.update((current) => current ? { ...current, isCompleted: false, completedAt: undefined } : current);
          this.toast.info('Отметка о прохождении снята');
        },
        error: (err) => {
          const e = err.error as ApiError | undefined;
          this.toast.error(e?.message ?? 'Не удалось обновить прогресс');
        },
      });
      return;
    }

    this.progressService.completeLesson(lessonId).subscribe({
      next: (response) => {
        this.manualProgress.set(response);
        this.toast.success('Урок отмечен как пройденный');
      },
      error: (err) => {
        const e = err.error as ApiError | undefined;
        this.toast.error(e?.message ?? 'Не удалось обновить прогресс');
      },
    });
  }

  onSubmitAnswer(blockId: string, answers: LessonBlockAnswer) {
    if (this.isPreview()) {
      this.toast.info('В режиме предпросмотра ответы не сохраняются');
      return;
    }
    this.attemptsService.submitAttempt(blockId, answers).subscribe({
      next: (result) => {
        this.attemptsService.getMyAttempt(blockId).subscribe({
          next: (attempt) => {
            this.entries.update((arr) =>
              arr.map((e) => (e.block.id === blockId ? { ...e, lastResult: result, attempt: attempt ?? null } : e)),
            );
          },
          error: () => {
            this.entries.update((arr) =>
              arr.map((e) => (e.block.id === blockId ? { ...e, lastResult: result } : e)),
            );
          },
        });
        this.loadProgress();
        if (result.isCorrect) {
          this.toast.success(`+${result.score} балла`);
        } else if (result.needsReview) {
          this.toast.info('Отправлено на проверку');
        }
      },
      error: (err) => {
        const e = err.error as ApiError | undefined;
        this.toast.error(e?.message ?? 'Не удалось отправить ответ');
      },
    });
  }
}

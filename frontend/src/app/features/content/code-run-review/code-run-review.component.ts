import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { BlockAttemptsService, ContentService } from '../services';
import { CodeExerciseBlockData, CodeExerciseRunDto, LessonBlockDto } from '../models';
import { CoursesService } from '../../courses/services/courses.service';
import { LessonDto } from '../../courses/models/course.model';

interface CodeBlockOption {
  id: string;
  label: string;
}

@Component({
  selector: 'app-code-run-review',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="review-page">
      <header class="review-page__header">
        <a class="review-page__back" [routerLink]="['/teacher/lesson', lessonId(), 'edit']">← К редактору урока</a>
        <div class="review-page__title-wrap">
          <h1 class="review-page__title">История запусков и отправок</h1>
          @if (lesson(); as lesson) {
            <p class="review-page__subtitle">{{ lesson.title }}</p>
          }
        </div>
        <button type="button" class="review-page__refresh" (click)="reloadRuns()" [disabled]="refreshing()">
          @if (refreshing()) { Обновляем… } @else { Обновить }
        </button>
      </header>

      <section class="review-page__filters">
        <label class="review-page__field">
          <span>Блок</span>
          <select [ngModel]="selectedBlockId()" (ngModelChange)="onBlockChange($event)">
            <option value="">Все code exercises</option>
            @for (block of codeBlocks(); track block.id) {
              <option [value]="block.id">{{ block.label }}</option>
            }
          </select>
        </label>

        <label class="review-page__field review-page__field--wide">
          <span>Поиск</span>
          <input
            type="text"
            [ngModel]="search()"
            (ngModelChange)="search.set($event)"
            placeholder="Студент, блок, язык, фрагмент кода"
          />
        </label>
      </section>

      @if (loading()) {
        <div class="review-page__state">Загрузка…</div>
      } @else if (codeBlocks().length === 0) {
        <div class="review-page__state">В уроке пока нет блоков Code Exercise.</div>
      } @else if (filteredRuns().length === 0) {
        <div class="review-page__state">По текущим фильтрам запусков не найдено.</div>
      } @else {
        <section class="review-page__list">
          @for (run of filteredRuns(); track run.id) {
            <article class="run-card">
              <header class="run-card__header">
                <div class="run-card__meta">
                  <span class="run-card__kind" [class.run-card__kind--submission]="run.kind === 'Submission'">
                    {{ run.kind === 'Submission' ? 'Отправка' : 'Запуск' }}
                  </span>
                  <span class="run-card__student">{{ run.userName || run.userId }}</span>
                  <span class="run-card__block">{{ run.blockLabel || ('Блок #' + ((run.blockOrderIndex ?? 0) + 1)) }}</span>
                </div>
                <div class="run-card__extra">
                  <span>{{ run.createdAt | date: 'dd.MM.y HH:mm:ss' }}</span>
                  <span>{{ run.language }}</span>
                  @if (run.attemptScore !== null && run.attemptScore !== undefined) {
                    <span class="run-card__score">{{ run.attemptScore }}/{{ run.attemptMaxScore }}</span>
                  }
                </div>
              </header>

              @if (run.globalError) {
                <div class="run-card__global-error">{{ run.globalError }}</div>
              }

              <pre class="run-card__code">{{ run.code }}</pre>

              <div class="run-card__results">
                @for (result of run.results; track $index) {
                  <div class="run-card__result" [class.run-card__result--ok]="result.passed" [class.run-card__result--fail]="!result.passed">
                    @if (result.isHidden) {
                      <span>Скрытый тест</span>
                    } @else {
                      <code>{{ result.input }} → {{ result.expectedOutput }}</code>
                    }
                    @if (result.actualOutput) {
                      <span class="run-card__actual">Вывод: <code>{{ result.actualOutput }}</code></span>
                    }
                    @if (result.error) {
                      <span class="run-card__error">{{ result.error }}</span>
                    }
                  </div>
                }
              </div>
            </article>
          }
        </section>
      }
    </div>
  `,
  styles: [
    `
      :host { display: block; }
      .review-page {
        display: flex;
        flex-direction: column;
        gap: 16px;
        padding: 24px;
      }
      .review-page__header {
        display: flex;
        align-items: center;
        gap: 16px;
        flex-wrap: wrap;
      }
      .review-page__back {
        color: #475569;
        text-decoration: none;
        font-size: 0.875rem;
      }
      .review-page__back:hover { color: #4F46E5; }
      .review-page__title-wrap { flex: 1; min-width: 220px; }
      .review-page__title {
        margin: 0;
        font-size: 1.5rem;
        color: #0F172A;
      }
      .review-page__subtitle {
        margin: 4px 0 0;
        color: #64748B;
      }
      .review-page__refresh {
        padding: 10px 14px;
        border: 1px solid #CBD5E1;
        border-radius: 10px;
        background: #FFFFFF;
        cursor: pointer;
        font-weight: 600;
      }
      .review-page__filters {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
        gap: 12px;
        padding: 16px;
        border: 1px solid #E2E8F0;
        border-radius: 14px;
        background: #FFFFFF;
      }
      .review-page__field {
        display: flex;
        flex-direction: column;
        gap: 6px;
        font-size: 0.875rem;
        color: #475569;
      }
      .review-page__field--wide { grid-column: span 2; }
      .review-page__field input,
      .review-page__field select {
        height: 42px;
        border: 1px solid #CBD5E1;
        border-radius: 10px;
        padding: 0 12px;
        background: #FFFFFF;
      }
      .review-page__state {
        padding: 32px;
        border: 1px dashed #CBD5E1;
        border-radius: 14px;
        text-align: center;
        color: #64748B;
        background: #FFFFFF;
      }
      .review-page__list {
        display: flex;
        flex-direction: column;
        gap: 14px;
      }
      .run-card {
        border: 1px solid #E2E8F0;
        border-radius: 16px;
        overflow: hidden;
        background: #FFFFFF;
      }
      .run-card__header {
        display: flex;
        justify-content: space-between;
        gap: 16px;
        flex-wrap: wrap;
        padding: 14px 16px;
        background: #F8FAFC;
        border-bottom: 1px solid #E2E8F0;
      }
      .run-card__meta,
      .run-card__extra {
        display: flex;
        align-items: center;
        gap: 10px;
        flex-wrap: wrap;
        font-size: 0.8125rem;
        color: #475569;
      }
      .run-card__kind {
        padding: 3px 9px;
        border-radius: 999px;
        background: #DCFCE7;
        color: #166534;
        font-weight: 700;
      }
      .run-card__kind--submission {
        background: #DBEAFE;
        color: #1D4ED8;
      }
      .run-card__student {
        font-weight: 700;
        color: #0F172A;
      }
      .run-card__score {
        font-weight: 700;
        color: #1D4ED8;
      }
      .run-card__global-error {
        padding: 12px 16px 0;
        color: #991B1B;
        font-size: 0.875rem;
      }
      .run-card__code {
        margin: 0;
        padding: 16px;
        background: #020617;
        color: #E2E8F0;
        font-size: 0.8125rem;
        line-height: 1.55;
        overflow-x: auto;
        font-family: ui-monospace, monospace;
      }
      .run-card__results {
        display: flex;
        flex-direction: column;
        gap: 8px;
        padding: 16px;
      }
      .run-card__result {
        display: flex;
        flex-direction: column;
        gap: 4px;
        padding: 10px 12px;
        border-radius: 10px;
        background: #F8FAFC;
        font-size: 0.8125rem;
      }
      .run-card__result--ok { background: #DCFCE7; }
      .run-card__result--fail { background: #FEE2E2; }
      .run-card__actual { color: #475569; }
      .run-card__error {
        color: #991B1B;
        font-family: ui-monospace, monospace;
      }
      @media (max-width: 760px) {
        .review-page { padding: 16px; }
        .review-page__field--wide { grid-column: span 1; }
      }
    `,
  ],
})
export class CodeRunReviewComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly coursesService = inject(CoursesService);
  private readonly contentService = inject(ContentService);
  private readonly attemptsService = inject(BlockAttemptsService);
  private readonly toast = inject(ToastService);

  readonly lessonId = signal('');
  readonly lesson = signal<LessonDto | null>(null);
  readonly codeBlocks = signal<CodeBlockOption[]>([]);
  readonly runs = signal<CodeExerciseRunDto[]>([]);
  readonly loading = signal(true);
  readonly refreshing = signal(false);
  readonly selectedBlockId = signal('');
  readonly search = signal('');

  readonly filteredRuns = computed(() => {
    const query = this.search().trim().toLowerCase();
    if (!query) return this.runs();

    return this.runs().filter((run) => {
      const haystack = [
        run.userName,
        run.userId,
        run.blockLabel,
        run.language,
        run.code,
      ]
        .filter(Boolean)
        .join(' ')
        .toLowerCase();

      return haystack.includes(query);
    });
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (!id) {
        this.loading.set(false);
        return;
      }

      this.lessonId.set(id);
      this.loadInitial();
    });
  }

  onBlockChange(blockId: string): void {
    this.selectedBlockId.set(blockId);
    this.reloadRuns();
  }

  reloadRuns(): void {
    if (!this.lessonId()) return;

    this.refreshing.set(true);
    this.attemptsService.getLessonCodeRuns(this.lessonId(), {
      blockId: this.selectedBlockId() || undefined,
      take: 200,
    }).subscribe({
      next: (runs) => {
        this.runs.set(runs);
        this.refreshing.set(false);
      },
      error: (err) => {
        this.refreshing.set(false);
        const apiError = err?.error as ApiError | undefined;
        this.toast.error(apiError?.message ?? 'Не удалось загрузить историю запусков');
      },
    });
  }

  private loadInitial(): void {
    this.loading.set(true);

    forkJoin({
      lesson: this.coursesService.getLessonById(this.lessonId()),
      blocks: this.contentService.getByLesson(this.lessonId()),
    }).subscribe({
      next: ({ lesson, blocks }) => {
        this.lesson.set(lesson);
        this.codeBlocks.set(this.buildCodeBlockOptions(blocks));
        this.loading.set(false);
        this.reloadRuns();
      },
      error: (err) => {
        this.loading.set(false);
        const apiError = err?.error as ApiError | undefined;
        this.toast.error(apiError?.message ?? 'Не удалось загрузить экран проверки');
      },
    });
  }

  private buildCodeBlockOptions(blocks: LessonBlockDto[]): CodeBlockOption[] {
    return blocks
      .filter((block) => block.type === 'CodeExercise')
      .sort((a, b) => a.orderIndex - b.orderIndex)
      .map((block) => {
        const data = block.data as CodeExerciseBlockData;
        const label = data?.instruction?.trim()
          ? `#${block.orderIndex + 1} · ${data.instruction.trim().slice(0, 72)}`
          : `#${block.orderIndex + 1} · Code Exercise`;

        return {
          id: block.id,
          label,
        };
      });
  }
}

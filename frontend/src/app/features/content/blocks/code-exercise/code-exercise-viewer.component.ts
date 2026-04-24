import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import {
  CodeExerciseRunDto,
  CodeExerciseBlockData,
  CodeExerciseAnswer,
  LessonBlockAttemptDto,
} from '../../models';
import {
  BlockAttemptsService,
  ContentService,
  CodeExecutionResponse,
  CodeExecutionCaseResult,
} from '../../services';
import { AuthService } from '../../../../core/services/auth.service';
import { UserRole } from '../../../../core/models/user.model';

@Component({
  selector: 'app-code-exercise-viewer',
  standalone: true,
  imports: [CommonModule, FormsModule, MonacoEditorModule],
  template: `
    <p class="instruction">{{ data.instruction }}</p>
    <div class="meta">Язык: {{ data.language }}</div>

    <ngx-monaco-editor
      class="code"
      [options]="monacoOptions"
      [ngModel]="code()"
      (ngModelChange)="code.set($event)"
    ></ngx-monaco-editor>

    <div class="actions">
      <button type="button" class="run" [disabled]="!code().trim() || running()" (click)="run()">
        @if (running()) { Запуск… } @else { ▶ Запустить }
      </button>
      @if (results().length > 0 || submitted()) {
        <button type="button" class="submit" [disabled]="!code().trim()" (click)="submit()">
          Отправить
        </button>
      }
    </div>

    @if (globalError()) {
      <div class="global-error">{{ globalError() }}</div>
    }

    @if (results().length > 0) {
      <div class="results">
        <div class="results__title">Результаты:</div>
        @for (r of results(); track $index) {
          <div class="result" [class.result--ok]="r.passed" [class.result--fail]="!r.passed">
            <div class="result__row">
              <span class="result__badge">{{ r.passed ? '✓' : '✗' }}</span>
              @if (r.isHidden) {
                <span class="result__hidden">Скрытый тест</span>
              } @else {
                <code class="result__io">{{ r.input }} → {{ r.expectedOutput }}</code>
              }
            </div>
            @if (!r.isHidden && r.actualOutput) {
              <div class="result__actual">Ваш вывод: <code>{{ r.actualOutput }}</code></div>
            }
            @if (r.error) {
              <div class="result__err">{{ r.error }}</div>
            }
          </div>
        }
      </div>
    }

    @if (history().length > 0) {
      <div class="history">
          <div class="history__head">
          <div class="history__title">История запусков</div>
          @if (history().length > 5) {
            <button type="button" class="history__toggle" (click)="toggleHistory()">
              {{ showAllHistory() ? 'Свернуть' : 'Показать всё' }}
            </button>
          }
        </div>

        @for (run of visibleHistory(); track run.id) {
          <details class="history-item">
            <summary class="history-item__summary">
              <span class="history-item__kind" [class.history-item__kind--submission]="run.kind === 'Submission'">
                {{ run.kind === 'Submission' ? 'Отправка' : 'Запуск' }}
              </span>
              <span class="history-item__time">{{ run.createdAt | date: 'dd.MM HH:mm:ss' }}</span>
              @if (run.attemptScore !== null && run.attemptScore !== undefined) {
                <span class="history-item__score">{{ run.attemptScore }}/{{ run.attemptMaxScore }}</span>
              }
              @if (!run.ok) {
                <span class="history-item__fail">Ошибка среды</span>
              }
            </summary>

            @if (run.globalError) {
              <div class="history-item__global-error">{{ run.globalError }}</div>
            }

            <pre class="history-item__code">{{ run.code }}</pre>

            <div class="history-item__results">
              @for (r of run.results; track $index) {
                <div class="history-item__result" [class.history-item__result--ok]="r.passed" [class.history-item__result--fail]="!r.passed">
                  @if (r.isHidden) {
                    <span>Скрытый тест</span>
                  } @else {
                    <code>{{ r.input }} → {{ r.expectedOutput }}</code>
                  }
                  @if (!r.isHidden && r.actualOutput) {
                    <span class="history-item__actual">Вывод: <code>{{ r.actualOutput }}</code></span>
                  }
                  @if (r.error) {
                    <span class="history-item__error">{{ r.error }}</span>
                  }
                </div>
              }
            </div>
          </details>
        }
      </div>
    }
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .instruction { margin: 0; font-size: 1rem; font-weight: 600; color: #0F172A; }
      .meta { font-size: 0.75rem; color: #64748B; }
      .code { display: block; height: 280px; border: 1px solid #E2E8F0; border-radius: 8px; overflow: hidden; }
      .actions { display: flex; gap: 8px; }
      .run, .submit {
        padding: 8px 20px; border: none; border-radius: 8px; cursor: pointer;
        font-weight: 600; font-size: 0.875rem;
      }
      .run { background: #10B981; color: #fff; }
      .run:hover:not(:disabled) { background: #059669; }
      .run:disabled { background: #CBD5E1; cursor: not-allowed; }
      .submit { background: #4F46E5; color: #fff; }
      .submit:hover:not(:disabled) { background: #4338CA; }
      .submit:disabled { background: #CBD5E1; cursor: not-allowed; }
      .global-error {
        padding: 10px 14px; background: #FEE2E2; color: #991B1B; border-radius: 8px; font-size: 0.875rem;
      }
      .results { display: flex; flex-direction: column; gap: 6px; }
      .results__title { font-size: 0.8125rem; font-weight: 600; color: #475569; }
      .result {
        padding: 8px 12px; border-radius: 8px; border: 1px solid #E2E8F0; background: #F8FAFC;
        font-size: 0.8125rem;
      }
      .result--ok { background: #DCFCE7; border-color: #10B981; }
      .result--fail { background: #FEE2E2; border-color: #EF4444; }
      .result__row { display: flex; align-items: center; gap: 8px; }
      .result__badge { font-weight: 700; }
      .result__hidden { color: #64748B; font-style: italic; }
      .result__io code, .result__actual code {
        font-family: ui-monospace, monospace; background: rgba(0,0,0,0.05); padding: 1px 6px; border-radius: 4px;
      }
      .result__actual { margin-top: 4px; color: #475569; }
      .result__err { margin-top: 4px; color: #991B1B; font-family: ui-monospace, monospace; font-size: 0.75rem; }
      .history {
        display: flex;
        flex-direction: column;
        gap: 10px;
        padding-top: 4px;
        border-top: 1px solid #E2E8F0;
      }
      .history__head {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 8px;
      }
      .history__title { font-size: 0.875rem; font-weight: 700; color: #0F172A; }
      .history__toggle {
        background: transparent;
        border: none;
        color: #4F46E5;
        cursor: pointer;
        font-size: 0.8125rem;
        font-weight: 600;
      }
      .history-item {
        border: 1px solid #E2E8F0;
        border-radius: 10px;
        background: #FFFFFF;
        overflow: hidden;
      }
      .history-item__summary {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: 8px;
        padding: 10px 12px;
        cursor: pointer;
        list-style: none;
        background: #F8FAFC;
      }
      .history-item__summary::-webkit-details-marker { display: none; }
      .history-item__kind {
        padding: 2px 8px;
        border-radius: 999px;
        font-size: 0.75rem;
        font-weight: 700;
        background: #DCFCE7;
        color: #166534;
      }
      .history-item__kind--submission {
        background: #DBEAFE;
        color: #1D4ED8;
      }
      .history-item__time,
      .history-item__score {
        font-size: 0.75rem;
        color: #475569;
      }
      .history-item__fail {
        font-size: 0.75rem;
        font-weight: 700;
        color: #B91C1C;
      }
      .history-item__global-error {
        padding: 10px 12px 0;
        color: #991B1B;
        font-size: 0.8125rem;
      }
      .history-item__code {
        margin: 0;
        padding: 12px;
        background: #020617;
        color: #E2E8F0;
        font-size: 0.75rem;
        line-height: 1.5;
        overflow-x: auto;
        font-family: ui-monospace, monospace;
      }
      .history-item__results {
        display: flex;
        flex-direction: column;
        gap: 6px;
        padding: 12px;
      }
      .history-item__result {
        display: flex;
        flex-direction: column;
        gap: 4px;
        padding: 8px 10px;
        border-radius: 8px;
        font-size: 0.75rem;
        background: #F8FAFC;
      }
      .history-item__result--ok { background: #DCFCE7; }
      .history-item__result--fail { background: #FEE2E2; }
      .history-item__actual { color: #475569; }
      .history-item__error {
        color: #991B1B;
        font-family: ui-monospace, monospace;
      }
    `,
  ],
})
export class CodeExerciseViewerComponent implements OnInit, OnChanges {
  @Input({ required: true }) data!: CodeExerciseBlockData;
  @Input() blockId = '';
  @Input() attempt: LessonBlockAttemptDto | null = null;
  @Input() showFeedback = true;
  @Output() submitAnswer = new EventEmitter<CodeExerciseAnswer>();

  private readonly contentService = inject(ContentService);
  private readonly attemptsService = inject(BlockAttemptsService);
  private readonly authService = inject(AuthService);

  code = signal('');
  running = signal(false);
  submitted = signal(false);
  results = signal<CodeExecutionCaseResult[]>([]);
  globalError = signal<string | null>(null);
  history = signal<CodeExerciseRunDto[]>([]);
  historyLoading = signal(false);
  showAllHistory = signal(false);
  readonly visibleHistory = computed(() =>
    this.showAllHistory() ? this.history() : this.history().slice(0, 5),
  );

  get monacoOptions() {
    return {
      theme: 'vs-dark',
      language: this.mapLang(this.data.language),
      minimap: { enabled: false },
      scrollBeyondLastLine: false,
      fontSize: 13,
      readOnly: this.submitted(),
    };
  }

  private mapLang(lang: string): string {
    const m: Record<string, string> = {
      python: 'python',
      javascript: 'javascript',
    };
    return m[lang] ?? 'plaintext';
  }

  ngOnInit() {
    this.syncFromAttempt();
    this.loadHistory();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['attempt'] || changes['data']) {
      this.syncFromAttempt();
    }

    if (changes['attempt'] || changes['blockId']) {
      this.loadHistory();
    }
  }

  run() {
    if (!this.blockId || !this.code().trim()) return;
    this.running.set(true);
    this.globalError.set(null);
    this.contentService.executeCode(this.blockId, this.code()).subscribe({
      next: (resp: CodeExecutionResponse) => {
        this.running.set(false);
        if (!resp.ok) {
          this.globalError.set(resp.globalError ?? 'Ошибка выполнения');
          this.results.set([]);
          this.loadHistory();
          return;
        }
        this.results.set(resp.results);
        this.loadHistory();
      },
      error: (err) => {
        this.running.set(false);
        this.globalError.set(err?.error?.message ?? 'Не удалось выполнить код');
      },
    });
  }

  submit() {
    if (!this.code().trim()) return;
    this.submitted.set(true);
    this.submitAnswer.emit({
      type: 'CodeExercise',
      code: this.code(),
      runOutput: this.results().map((r) => ({
        input: r.input,
        expectedOutput: r.expectedOutput,
        actualOutput: r.actualOutput,
        passed: r.passed,
        isHidden: r.isHidden,
      })),
    });
  }

  private syncFromAttempt(): void {
    if (this.attempt?.answers && this.attempt.answers.type === 'CodeExercise') {
      this.code.set(this.attempt.answers.code);
      this.submitted.set(true);
      this.results.set(
        (this.attempt.answers.runOutput ?? []).map((r) => ({
          input: r.input,
          expectedOutput: r.expectedOutput,
          actualOutput: r.actualOutput,
          passed: r.passed,
          isHidden: r.isHidden,
          error: null,
        })),
      );
      return;
    }

    if (!this.code()) {
      this.code.set(this.data.starterCode ?? '');
    }
  }

  private loadHistory(): void {
    if (!this.blockId || this.authService.userRole() !== UserRole.Student) {
      this.history.set([]);
      return;
    }

    this.historyLoading.set(true);
    this.attemptsService.getMyCodeRuns(this.blockId, 10).subscribe({
      next: (runs) => {
        this.history.set(runs);
        this.historyLoading.set(false);
      },
      error: () => {
        this.historyLoading.set(false);
      },
    });
  }

  toggleHistory(): void {
    this.showAllHistory.update((value) => !value);
  }
}

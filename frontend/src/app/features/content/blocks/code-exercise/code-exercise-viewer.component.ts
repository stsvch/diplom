import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import {
  CodeExerciseBlockData,
  CodeExerciseAnswer,
  LessonBlockAttemptDto,
} from '../../models';
import {
  ContentService,
  CodeExecutionResponse,
  CodeExecutionCaseResult,
} from '../../services';

@Component({
  selector: 'app-code-exercise-viewer',
  standalone: true,
  imports: [FormsModule, MonacoEditorModule],
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
    `,
  ],
})
export class CodeExerciseViewerComponent implements OnInit {
  @Input({ required: true }) data!: CodeExerciseBlockData;
  @Input() blockId = '';
  @Input() attempt: LessonBlockAttemptDto | null = null;
  @Input() showFeedback = true;
  @Output() submitAnswer = new EventEmitter<CodeExerciseAnswer>();

  private readonly contentService = inject(ContentService);

  code = signal('');
  running = signal(false);
  submitted = signal(false);
  results = signal<CodeExecutionCaseResult[]>([]);
  globalError = signal<string | null>(null);

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
      csharp: 'csharp', python: 'python', javascript: 'javascript', java: 'java', cpp: 'cpp',
    };
    return m[lang] ?? 'plaintext';
  }

  ngOnInit() {
    if (this.attempt?.answers && this.attempt.answers.type === 'CodeExercise') {
      this.code.set(this.attempt.answers.code);
      this.submitted.set(true);
    } else {
      this.code.set(this.data.starterCode ?? '');
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
          return;
        }
        this.results.set(resp.results);
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
      })),
    });
  }
}

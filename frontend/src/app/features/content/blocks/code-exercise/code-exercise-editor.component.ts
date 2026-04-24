import { Component, EventEmitter, Input, Output, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import { CodeExerciseBlockData, CodeTestCase } from '../../models';

const LANGUAGES = [
  { value: 'javascript', label: 'JavaScript' },
  { value: 'python', label: 'Python' },
];

@Component({
  selector: 'app-code-exercise-editor',
  standalone: true,
  imports: [FormsModule, LucideAngularModule, MonacoEditorModule],
  template: `
    <input
      type="text" class="input input--title" placeholder="Инструкция"
      [ngModel]="data.instruction"
      (ngModelChange)="update({ instruction: $event })"
    />

    <div class="row">
      <label class="field">
        <span>Язык</span>
        <select class="input" [ngModel]="data.language" (ngModelChange)="update({ language: $event })">
          @for (l of languages; track l.value) {
            <option [value]="l.value">{{ l.label }}</option>
          }
        </select>
      </label>
      <label class="field">
        <span>Таймаут (мс)</span>
        <input type="number" class="input" min="500" max="60000"
          [ngModel]="data.timeoutMs"
          (ngModelChange)="update({ timeoutMs: toNum($event, 5000) })"
        />
      </label>
    </div>

    <div class="field">
      <span>Стартовый код</span>
      <ngx-monaco-editor
        class="code"
        [options]="monacoOptions()"
        [ngModel]="data.starterCode ?? ''"
        (ngModelChange)="update({ starterCode: $event })"
      ></ngx-monaco-editor>
    </div>

    <div class="tests">
      <div class="tests__title">Тест-кейсы</div>
      @for (tc of data.testCases; track $index; let i = $index) {
        <div class="test">
          <input type="text" class="input" placeholder="Вход"
            [ngModel]="tc.input"
            (ngModelChange)="updateTest(i, { input: $event })"
          />
          <input type="text" class="input" placeholder="Ожидаемый выход"
            [ngModel]="tc.expectedOutput"
            (ngModelChange)="updateTest(i, { expectedOutput: $event })"
          />
          <label class="hidden-toggle">
            <input type="checkbox" [checked]="tc.isHidden" (change)="updateTest(i, { isHidden: $any($event.target).checked })" />
            скрытый
          </label>
          <button type="button" class="remove" (click)="removeTest(i)">
            <lucide-icon name="x" size="14"></lucide-icon>
          </button>
        </div>
      }
      <button type="button" class="add" (click)="addTest()">
        <lucide-icon name="plus" size="14"></lucide-icon>
        Добавить тест
      </button>
    </div>
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .input {
        padding: 8px 12px; border: 1px solid #E2E8F0; border-radius: 8px;
        background: #F8FAFC; font: inherit; font-size: 0.875rem; outline: none;
      }
      .input:focus { border-color: #4F46E5; background: #fff; }
      .input--title { font-size: 1rem; font-weight: 600; }
      .code { display: block; height: 200px; border: 1px solid #E2E8F0; border-radius: 8px; overflow: hidden; }
      .row { display: flex; gap: 12px; }
      .field { display: flex; flex-direction: column; gap: 4px; font-size: 0.75rem; color: #64748B; flex: 1; }
      .tests { display: flex; flex-direction: column; gap: 6px; padding: 12px; background: #F8FAFC; border-radius: 8px; }
      .tests__title { font-size: 0.8125rem; font-weight: 600; color: #475569; }
      .test { display: grid; grid-template-columns: 1fr 1fr auto auto; gap: 6px; align-items: center; }
      .hidden-toggle { font-size: 0.75rem; color: #64748B; display: inline-flex; align-items: center; gap: 4px; }
      .remove {
        width: 24px; height: 24px; border: none; background: transparent; color: #64748B;
        cursor: pointer; border-radius: 4px; display: inline-flex; align-items: center; justify-content: center;
      }
      .remove:hover { background: #FEE2E2; color: #EF4444; }
      .add {
        align-self: flex-start; display: inline-flex; align-items: center; gap: 4px;
        padding: 4px 10px; border: 1px dashed #CBD5E1; background: transparent;
        border-radius: 6px; cursor: pointer; color: #64748B; font-size: 0.75rem;
      }
      .add:hover { color: #4F46E5; border-color: #4F46E5; }
    `,
  ],
})
export class CodeExerciseEditorComponent {
  @Input({ required: true }) data!: CodeExerciseBlockData;
  @Output() dataChange = new EventEmitter<CodeExerciseBlockData>();

  languages = LANGUAGES;

  monacoOptions = computed(() => ({
    theme: 'vs-dark',
    language: this.mapLang(this.data?.language ?? 'csharp'),
    minimap: { enabled: false },
    scrollBeyondLastLine: false,
    fontSize: 13,
  }));

  private mapLang(lang: string): string {
    const m: Record<string, string> = {
      python: 'python',
      javascript: 'javascript',
    };
    return m[lang] ?? 'plaintext';
  }

  update(patch: Partial<CodeExerciseBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }

  updateTest(i: number, patch: Partial<CodeTestCase>) {
    const testCases = this.data.testCases.map((t, idx) => (idx === i ? { ...t, ...patch } : t));
    this.update({ testCases });
  }

  addTest() {
    this.update({ testCases: [...this.data.testCases, { input: '', expectedOutput: '', isHidden: false }] });
  }

  removeTest(i: number) {
    this.update({ testCases: this.data.testCases.filter((_, idx) => idx !== i) });
  }

  toNum(v: string | number, fallback: number): number {
    const n = Number(v);
    return isNaN(n) ? fallback : n;
  }
}

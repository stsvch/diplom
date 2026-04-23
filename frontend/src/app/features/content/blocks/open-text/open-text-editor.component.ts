import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { OpenTextBlockData, OpenTextLengthUnit } from '../../models';

@Component({
  selector: 'app-open-text-editor',
  standalone: true,
  imports: [FormsModule],
  template: `
    <input
      type="text" class="input input--title" placeholder="Инструкция"
      [ngModel]="data.instruction"
      (ngModelChange)="update({ instruction: $event })"
    />
    <textarea
      class="input textarea" rows="3" placeholder="Уточняющие вопросы / подсказки"
      [ngModel]="data.prompt"
      (ngModelChange)="update({ prompt: $event })"
    ></textarea>
    <input
      type="text" class="input" placeholder="Вспомогательные слова через запятую"
      [ngModel]="joinArr(data.helperWords)"
      (ngModelChange)="update({ helperWords: splitArr($event) })"
    />

    <div class="row">
      <label class="field">
        <span>Единица</span>
        <select class="input"
          [ngModel]="data.unit"
          (ngModelChange)="update({ unit: $event })"
        >
          <option value="Chars">Символы</option>
          <option value="Words">Слова</option>
        </select>
      </label>
      <label class="field">
        <span>Мин</span>
        <input type="number" class="input"
          [ngModel]="data.minLength"
          (ngModelChange)="update({ minLength: toNum($event) })"
        />
      </label>
      <label class="field">
        <span>Макс</span>
        <input type="number" class="input"
          [ngModel]="data.maxLength"
          (ngModelChange)="update({ maxLength: toNum($event) })"
        />
      </label>
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
      .textarea { resize: vertical; }
      .row { display: flex; gap: 12px; }
      .field { display: flex; flex-direction: column; gap: 4px; font-size: 0.75rem; color: #64748B; flex: 1; }
    `,
  ],
})
export class OpenTextEditorComponent {
  @Input({ required: true }) data!: OpenTextBlockData;
  @Output() dataChange = new EventEmitter<OpenTextBlockData>();

  update(patch: Partial<OpenTextBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }

  joinArr(arr: string[]): string {
    return arr.join(', ');
  }

  splitArr(str: string): string[] {
    return str.split(',').map((s) => s.trim()).filter(Boolean);
  }

  toNum(v: string | number | null | undefined): number | undefined {
    if (v === null || v === undefined || v === '') return undefined;
    const n = Number(v);
    return isNaN(n) ? undefined : n;
  }
}

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { WordBankBlockData, WordBankSentence } from '../../models';

@Component({
  selector: 'app-word-bank-editor',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  template: `
    <input
      type="text" class="input" placeholder="Инструкция (опц.)"
      [ngModel]="data.instruction"
      (ngModelChange)="update({ instruction: $event })"
    />

    <div class="field">
      <span class="label">Банк слов (через запятую)</span>
      <input
        type="text" class="input"
        placeholder="hydrated, released, deal with, aid..."
        [ngModel]="joinArr(data.bank)"
        (ngModelChange)="update({ bank: splitArr($event) })"
      />
    </div>

    <label class="toggle">
      <input type="checkbox" [checked]="data.allowExtraWords" (change)="update({ allowExtraWords: $any($event.target).checked })" />
      <span>В банке могут быть лишние слова</span>
    </label>

    <p class="hint">Используйте <code>{{ '{{0}}' }}</code>, <code>{{ '{{1}}' }}</code> в шаблоне</p>

    <div class="sentences">
      @for (s of data.sentences; track s.id; let i = $index) {
        <div class="sentence">
          <div class="row">
            <span class="num">{{ i + 1 }}.</span>
            <input
              type="text" class="input template"
              placeholder="help to {{ '{{0}}' }} stress and {{ '{{1}}' }} effectively."
              [ngModel]="s.template"
              (ngModelChange)="updateSentence(i, { template: $event })"
            />
            <button type="button" class="remove" (click)="removeSentence(i)" [disabled]="data.sentences.length <= 1">
              <lucide-icon name="x" size="14"></lucide-icon>
            </button>
          </div>

          <div class="row row--offset">
            <span class="label">Правильные</span>
            <input
              type="text" class="input"
              placeholder="deal with, aid"
              [ngModel]="joinArr(s.correctAnswers)"
              (ngModelChange)="updateSentence(i, { correctAnswers: splitArr($event) })"
            />
          </div>
        </div>
      }
      <button type="button" class="add" (click)="addSentence()">
        <lucide-icon name="plus" size="14"></lucide-icon>
        Добавить предложение
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
      .field { display: flex; flex-direction: column; gap: 4px; }
      .label { font-size: 0.75rem; color: #64748B; }
      .toggle { display: inline-flex; align-items: center; gap: 8px; font-size: 0.875rem; color: #4B5563; }
      .hint { margin: 0; font-size: 0.75rem; color: #64748B; }
      .hint code { background: #F1F5F9; padding: 1px 4px; border-radius: 4px; }
      .sentences { display: flex; flex-direction: column; gap: 12px; }
      .sentence { padding: 12px; background: #F8FAFC; border: 1px solid #E2E8F0; border-radius: 10px; display: flex; flex-direction: column; gap: 8px; }
      .row { display: flex; align-items: center; gap: 8px; }
      .row--offset { padding-left: 32px; }
      .num { font-weight: 600; color: #64748B; min-width: 24px; }
      .template { flex: 1; }
      .remove {
        width: 24px; height: 24px; border: none; background: transparent; color: #64748B;
        cursor: pointer; border-radius: 4px; display: inline-flex; align-items: center; justify-content: center;
      }
      .remove:hover:not(:disabled) { background: #FEE2E2; color: #EF4444; }
      .remove:disabled { opacity: 0.3; cursor: not-allowed; }
      .add {
        align-self: flex-start; display: inline-flex; align-items: center; gap: 4px;
        padding: 6px 12px; border: 1px dashed #CBD5E1; background: transparent;
        border-radius: 8px; cursor: pointer; color: #64748B; font-size: 0.875rem;
      }
      .add:hover { color: #4F46E5; border-color: #4F46E5; }
    `,
  ],
})
export class WordBankEditorComponent {
  @Input({ required: true }) data!: WordBankBlockData;
  @Output() dataChange = new EventEmitter<WordBankBlockData>();

  update(patch: Partial<WordBankBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }

  updateSentence(i: number, patch: Partial<WordBankSentence>) {
    const sentences = this.data.sentences.map((s, idx) => (idx === i ? { ...s, ...patch } : s));
    this.update({ sentences });
  }

  addSentence() {
    const used = new Set(this.data.sentences.map((s) => s.id));
    let n = 1;
    while (used.has(`s${n}`)) n++;
    this.update({ sentences: [...this.data.sentences, { id: `s${n}`, template: '', correctAnswers: [] }] });
  }

  removeSentence(i: number) {
    if (this.data.sentences.length <= 1) return;
    this.update({ sentences: this.data.sentences.filter((_, idx) => idx !== i) });
  }

  joinArr(arr: string[]): string {
    return arr.join(', ');
  }

  splitArr(str: string): string[] {
    return str.split(',').map((s) => s.trim()).filter((s) => s.length > 0);
  }
}

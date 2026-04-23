import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { FillGapBlockData, FillGapSentence, FillGapSlot } from '../../models';

@Component({
  selector: 'app-fill-gap-editor',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  template: `
    <input
      type="text" class="input" placeholder="Инструкция (опц.)"
      [ngModel]="data.instruction"
      (ngModelChange)="update({ instruction: $event })"
    />

    <p class="hint">
      В шаблоне используйте <code>{{ '{{0}}' }}</code>, <code>{{ '{{1}}' }}</code>... для пропусков.
    </p>

    <div class="sentences">
      @for (s of data.sentences; track s.id; let i = $index) {
        <div class="sentence">
          <div class="sentence__head">
            <span class="sentence__num">{{ i + 1 }}.</span>
            <input
              type="text" class="input sentence__template"
              placeholder="If you {{ '{{0}}' }} a healthy diet, you {{ '{{1}}' }} fit."
              [ngModel]="s.template"
              (ngModelChange)="updateSentence(i, { template: $event })"
            />
            <button type="button" class="remove" (click)="removeSentence(i)" [disabled]="data.sentences.length <= 1">
              <lucide-icon name="x" size="14"></lucide-icon>
            </button>
          </div>

          <div class="gaps">
            @for (g of s.gaps; track g.id; let gi = $index) {
              <div class="gap">
                <span class="gap__label">Пропуск {{ '{{' }}{{ g.id }}{{ '}}' }}</span>
                <input
                  type="text" class="input gap__answers"
                  placeholder="правильные ответы через запятую"
                  [ngModel]="joinAnswers(g.correctAnswers)"
                  (ngModelChange)="updateGap(i, gi, { correctAnswers: splitAnswers($event) })"
                />
                <label class="gap__case">
                  <input type="checkbox" [checked]="g.caseSensitive" (change)="updateGap(i, gi, { caseSensitive: $any($event.target).checked })" />
                  A/a
                </label>
                <button type="button" class="remove-small" (click)="removeGap(i, gi)" [disabled]="s.gaps.length <= 1">
                  <lucide-icon name="x" size="12"></lucide-icon>
                </button>
              </div>
            }
            <button type="button" class="add-small" (click)="addGap(i)">
              <lucide-icon name="plus" size="12"></lucide-icon> пропуск
            </button>
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
      .hint { margin: 0; color: #64748B; font-size: 0.75rem; }
      .hint code { background: #F1F5F9; padding: 1px 4px; border-radius: 4px; }
      .sentences { display: flex; flex-direction: column; gap: 16px; }
      .sentence {
        padding: 12px; background: #F8FAFC; border: 1px solid #E2E8F0; border-radius: 10px;
        display: flex; flex-direction: column; gap: 8px;
      }
      .sentence__head { display: flex; align-items: center; gap: 8px; }
      .sentence__num { font-weight: 600; color: #64748B; min-width: 24px; }
      .sentence__template { flex: 1; }
      .gaps { display: flex; flex-direction: column; gap: 4px; padding-left: 32px; }
      .gap { display: flex; align-items: center; gap: 8px; font-size: 0.8125rem; }
      .gap__label { color: #64748B; min-width: 88px; }
      .gap__answers { flex: 1; font-size: 0.8125rem; }
      .gap__case { display: inline-flex; align-items: center; gap: 4px; color: #64748B; font-size: 0.75rem; cursor: pointer; }
      .remove, .remove-small {
        border: none; background: transparent; color: #64748B; cursor: pointer;
        display: inline-flex; align-items: center; justify-content: center; border-radius: 4px;
      }
      .remove { width: 24px; height: 24px; }
      .remove-small { width: 20px; height: 20px; }
      .remove:hover:not(:disabled), .remove-small:hover:not(:disabled) {
        background: #FEE2E2; color: #EF4444;
      }
      .remove:disabled, .remove-small:disabled { opacity: 0.3; cursor: not-allowed; }
      .add, .add-small {
        align-self: flex-start; display: inline-flex; align-items: center; gap: 4px;
        border: 1px dashed #CBD5E1; background: transparent;
        border-radius: 8px; cursor: pointer; color: #64748B;
      }
      .add { padding: 6px 12px; font-size: 0.875rem; }
      .add-small { padding: 2px 8px; font-size: 0.75rem; }
      .add:hover, .add-small:hover { color: #4F46E5; border-color: #4F46E5; }
    `,
  ],
})
export class FillGapEditorComponent {
  @Input({ required: true }) data!: FillGapBlockData;
  @Output() dataChange = new EventEmitter<FillGapBlockData>();

  update(patch: Partial<FillGapBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }

  updateSentence(i: number, patch: Partial<FillGapSentence>) {
    const sentences = this.data.sentences.map((s, idx) => (idx === i ? { ...s, ...patch } : s));
    this.update({ sentences });
  }

  updateGap(si: number, gi: number, patch: Partial<FillGapSlot>) {
    const sentences = this.data.sentences.map((s, idx) => {
      if (idx !== si) return s;
      const gaps = s.gaps.map((g, gidx) => (gidx === gi ? { ...g, ...patch } : g));
      return { ...s, gaps };
    });
    this.update({ sentences });
  }

  addSentence() {
    const used = new Set(this.data.sentences.map((s) => s.id));
    let n = 1;
    while (used.has(`s${n}`)) n++;
    this.update({
      sentences: [
        ...this.data.sentences,
        { id: `s${n}`, template: '', gaps: [{ id: '0', correctAnswers: [''], caseSensitive: false }] },
      ],
    });
  }

  removeSentence(i: number) {
    if (this.data.sentences.length <= 1) return;
    this.update({ sentences: this.data.sentences.filter((_, idx) => idx !== i) });
  }

  addGap(si: number) {
    const sentence = this.data.sentences[si];
    const used = new Set(sentence.gaps.map((g) => g.id));
    let n = 0;
    while (used.has(String(n))) n++;
    this.updateSentence(si, {
      gaps: [...sentence.gaps, { id: String(n), correctAnswers: [''], caseSensitive: false }],
    });
  }

  removeGap(si: number, gi: number) {
    const sentence = this.data.sentences[si];
    if (sentence.gaps.length <= 1) return;
    this.updateSentence(si, { gaps: sentence.gaps.filter((_, idx) => idx !== gi) });
  }

  joinAnswers(arr: string[]): string {
    return arr.join(', ');
  }

  splitAnswers(str: string): string[] {
    return str.split(',').map((s) => s.trim()).filter((s) => s.length > 0);
  }
}

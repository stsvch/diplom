import { Component, EventEmitter, Input, OnInit, Output, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  WordBankBlockData,
  WordBankAnswer,
  WordBankResponse,
  LessonBlockAttemptDto,
} from '../../models';

interface Segment { kind: 'text' | 'gap'; value: string }

@Component({
  selector: 'app-word-bank-viewer',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (data.instruction) { <p class="instruction">{{ data.instruction }}</p> }

    <div class="bank">
      @for (w of data.bank; track w) {
        <span class="chip" [class.chip--used]="isUsed(w) && !data.allowExtraWords">{{ w }}</span>
      }
    </div>

    <ol class="sentences">
      @for (s of data.sentences; track s.id; let si = $index) {
        <li class="sentence">
          @for (seg of splitTemplate(s.template); track $index) {
            @if (seg.kind === 'text') { <span>{{ seg.value }}</span> }
            @else {
              <select
                class="gap"
                [class.gap--correct]="isCorrect(s.id, +seg.value)"
                [class.gap--wrong]="isWrong(s.id, +seg.value)"
                [disabled]="submitted()"
                [ngModel]="getValue(s.id, +seg.value)"
                (ngModelChange)="setValue(s.id, +seg.value, $event)"
              >
                <option value="">...</option>
                @for (w of data.bank; track w) { <option [value]="w">{{ w }}</option> }
              </select>
            }
          }
        </li>
      }
    </ol>

    @if (!submitted()) {
      <button type="button" class="submit" [disabled]="!allFilled()" (click)="submit()">Проверить</button>
    }
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .instruction { margin: 0; color: #64748B; font-size: 0.875rem; }
      .bank { display: flex; flex-wrap: wrap; gap: 6px; padding: 12px; background: #F8FAFC; border-radius: 8px; }
      .chip { padding: 4px 10px; background: #fff; border: 1px solid #E2E8F0; border-radius: 999px; font-size: 0.8125rem; color: #0F172A; }
      .chip--used { opacity: 0.4; text-decoration: line-through; }
      .sentences { margin: 0; padding-left: 24px; display: flex; flex-direction: column; gap: 12px; }
      .sentence { line-height: 1.9; }
      .gap { display: inline-block; padding: 2px 8px; margin: 0 4px; border: 1px solid #CBD5E1; border-radius: 6px; font: inherit; min-width: 120px; }
      .gap:focus { border-color: #4F46E5; outline: none; }
      .gap--correct { background: #DCFCE7; border-color: #10B981; }
      .gap--wrong { background: #FEE2E2; border-color: #EF4444; }
      .submit {
        align-self: flex-start; padding: 8px 20px; border: none;
        background: #4F46E5; color: #fff; border-radius: 8px; cursor: pointer;
        font-weight: 600; font-size: 0.875rem;
      }
      .submit:disabled { background: #CBD5E1; cursor: not-allowed; }
    `,
  ],
})
export class WordBankViewerComponent implements OnInit {
  @Input({ required: true }) data!: WordBankBlockData;
  @Input() attempt: LessonBlockAttemptDto | null = null;
  @Input() showFeedback = true;
  @Output() submitAnswer = new EventEmitter<WordBankAnswer>();

  values = signal<Map<string, string>>(new Map()); // "sentenceId|gapIndex" -> word
  submitted = signal(false);

  showResult = computed(() => this.submitted() && this.showFeedback);

  ngOnInit() {
    if (this.attempt?.answers && this.attempt.answers.type === 'WordBank') {
      const map = new Map<string, string>();
      for (const r of this.attempt.answers.responses) {
        r.answers.forEach((a, i) => map.set(`${r.sentenceId}|${i}`, a));
      }
      this.values.set(map);
      this.submitted.set(true);
    }
  }

  splitTemplate(template: string): Segment[] {
    const segments: Segment[] = [];
    const regex = /\{\{(\d+)\}\}/g;
    let last = 0;
    let m: RegExpExecArray | null;
    while ((m = regex.exec(template)) !== null) {
      if (m.index > last) segments.push({ kind: 'text', value: template.slice(last, m.index) });
      segments.push({ kind: 'gap', value: m[1] });
      last = m.index + m[0].length;
    }
    if (last < template.length) segments.push({ kind: 'text', value: template.slice(last) });
    return segments;
  }

  getValue(sentenceId: string, idx: number): string {
    return this.values().get(`${sentenceId}|${idx}`) ?? '';
  }

  setValue(sentenceId: string, idx: number, v: string) {
    const map = new Map(this.values());
    map.set(`${sentenceId}|${idx}`, v);
    this.values.set(map);
  }

  isUsed(word: string): boolean {
    return Array.from(this.values().values()).includes(word);
  }

  allFilled(): boolean {
    for (const s of this.data.sentences) {
      for (let i = 0; i < s.correctAnswers.length; i++) {
        if (!this.getValue(s.id, i)) return false;
      }
    }
    return true;
  }

  isCorrect(sentenceId: string, idx: number): boolean {
    if (!this.showResult()) return false;
    const s = this.data.sentences.find((x) => x.id === sentenceId);
    if (!s) return false;
    const expected = s.correctAnswers[idx];
    return this.getValue(sentenceId, idx).trim().toLowerCase() === (expected ?? '').trim().toLowerCase();
  }

  isWrong(sentenceId: string, idx: number): boolean {
    return this.showResult() && !this.isCorrect(sentenceId, idx);
  }

  submit() {
    if (!this.allFilled()) return;
    this.submitted.set(true);
    const responses: WordBankResponse[] = this.data.sentences.map((s) => ({
      sentenceId: s.id,
      answers: s.correctAnswers.map((_, i) => this.getValue(s.id, i)),
    }));
    this.submitAnswer.emit({ type: 'WordBank', responses });
  }
}

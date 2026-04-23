import { Component, EventEmitter, Input, OnInit, Output, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  FillGapBlockData,
  FillGapAnswer,
  FillGapResponse,
  LessonBlockAttemptDto,
} from '../../models';

interface Segment {
  kind: 'text' | 'gap';
  value: string; // text or gapId
}

@Component({
  selector: 'app-fill-gap-viewer',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (data.instruction) { <p class="instruction">{{ data.instruction }}</p> }

    <ol class="sentences">
      @for (s of data.sentences; track s.id; let si = $index) {
        <li class="sentence">
          @for (seg of splitTemplate(s.template); track $index) {
            @if (seg.kind === 'text') {
              <span>{{ seg.value }}</span>
            } @else {
              <input
                type="text"
                class="gap"
                [class.gap--correct]="isGapCorrect(s.id, seg.value)"
                [class.gap--wrong]="isGapWrong(s.id, seg.value)"
                [disabled]="submitted()"
                [ngModel]="getValue(s.id, seg.value)"
                (ngModelChange)="setValue(s.id, seg.value, $event)"
              />
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
      .sentences { margin: 0; padding-left: 24px; display: flex; flex-direction: column; gap: 12px; }
      .sentence { line-height: 1.8; }
      .gap {
        display: inline-block; width: 120px; padding: 2px 8px; margin: 0 4px;
        border: none; border-bottom: 2px solid #CBD5E1; background: transparent;
        font: inherit; color: #0F172A; text-align: center; outline: none;
      }
      .gap:focus { border-bottom-color: #4F46E5; }
      .gap--correct { background: #DCFCE7; border-bottom-color: #10B981; color: #166534; }
      .gap--wrong { background: #FEE2E2; border-bottom-color: #EF4444; color: #991B1B; }
      .submit {
        align-self: flex-start; padding: 8px 20px; border: none;
        background: #4F46E5; color: #fff; border-radius: 8px; cursor: pointer;
        font-weight: 600; font-size: 0.875rem;
      }
      .submit:disabled { background: #CBD5E1; cursor: not-allowed; }
    `,
  ],
})
export class FillGapViewerComponent implements OnInit {
  @Input({ required: true }) data!: FillGapBlockData;
  @Input() attempt: LessonBlockAttemptDto | null = null;
  @Input() showFeedback = true;
  @Output() submitAnswer = new EventEmitter<FillGapAnswer>();

  values = signal<Map<string, string>>(new Map()); // "sentenceId|gapId" -> value
  submitted = signal(false);

  showResult = computed(() => this.submitted() && this.showFeedback);

  ngOnInit() {
    if (this.attempt?.answers && 'responses' in this.attempt.answers && this.attempt.answers.type !== 'Dropdown') {
      const ans = this.attempt.answers as FillGapAnswer;
      const map = new Map<string, string>();
      for (const r of ans.responses) {
        for (const g of r.gaps) map.set(`${r.sentenceId}|${g.gapId}`, g.value);
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

  getValue(sentenceId: string, gapId: string): string {
    return this.values().get(`${sentenceId}|${gapId}`) ?? '';
  }

  setValue(sentenceId: string, gapId: string, val: string) {
    const map = new Map(this.values());
    map.set(`${sentenceId}|${gapId}`, val);
    this.values.set(map);
  }

  allFilled(): boolean {
    for (const s of this.data.sentences) {
      for (const g of s.gaps) {
        if (!this.getValue(s.id, g.id).trim()) return false;
      }
    }
    return true;
  }

  isGapCorrect(sentenceId: string, gapId: string): boolean {
    if (!this.showResult()) return false;
    const sentence = this.data.sentences.find((s) => s.id === sentenceId);
    const gap = sentence?.gaps.find((g) => g.id === gapId);
    if (!gap) return false;
    const value = this.getValue(sentenceId, gapId).trim();
    return gap.correctAnswers.some((a) =>
      gap.caseSensitive ? a.trim() === value : a.trim().toLowerCase() === value.toLowerCase(),
    );
  }

  isGapWrong(sentenceId: string, gapId: string): boolean {
    return this.showResult() && !this.isGapCorrect(sentenceId, gapId);
  }

  submit() {
    if (!this.allFilled()) return;
    this.submitted.set(true);
    const responses: FillGapResponse[] = this.data.sentences.map((s) => ({
      sentenceId: s.id,
      gaps: s.gaps.map((g) => ({ gapId: g.id, value: this.getValue(s.id, g.id) })),
    }));
    this.submitAnswer.emit({ type: 'FillGap', responses });
  }
}

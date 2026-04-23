import { Component, EventEmitter, Input, OnInit, Output, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  DropdownBlockData,
  DropdownAnswer,
  FillGapResponse,
  LessonBlockAttemptDto,
} from '../../models';

interface Segment {
  kind: 'text' | 'gap';
  value: string;
}

@Component({
  selector: 'app-dropdown-viewer',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (data.instruction) { <p class="instruction">{{ data.instruction }}</p> }

    <ol class="sentences">
      @for (s of data.sentences; track s.id) {
        <li class="sentence">
          @for (seg of splitTemplate(s.template); track $index) {
            @if (seg.kind === 'text') {
              <span>{{ seg.value }}</span>
            } @else {
              <select
                class="gap"
                [class.gap--correct]="isCorrect(s.id, seg.value)"
                [class.gap--wrong]="isWrong(s.id, seg.value)"
                [disabled]="submitted()"
                [ngModel]="getValue(s.id, seg.value)"
                (ngModelChange)="setValue(s.id, seg.value, $event)"
              >
                <option value="" disabled selected>...</option>
                @for (opt of getOptions(s.id, seg.value); track opt) {
                  <option [value]="opt">{{ opt }}</option>
                }
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
      .sentences { margin: 0; padding-left: 24px; display: flex; flex-direction: column; gap: 12px; }
      .sentence { line-height: 1.8; }
      .gap {
        display: inline-block; min-width: 120px; padding: 4px 8px; margin: 0 4px;
        border: 1px solid #CBD5E1; border-radius: 6px; background: #fff; font: inherit;
      }
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
export class DropdownViewerComponent implements OnInit {
  @Input({ required: true }) data!: DropdownBlockData;
  @Input() attempt: LessonBlockAttemptDto | null = null;
  @Input() showFeedback = true;
  @Output() submitAnswer = new EventEmitter<DropdownAnswer>();

  values = signal<Map<string, string>>(new Map());
  submitted = signal(false);

  showResult = computed(() => this.submitted() && this.showFeedback);

  ngOnInit() {
    if (this.attempt?.answers && this.attempt.answers.type === 'Dropdown') {
      const map = new Map<string, string>();
      for (const r of this.attempt.answers.responses) {
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

  getOptions(sentenceId: string, gapId: string): string[] {
    const s = this.data.sentences.find((x) => x.id === sentenceId);
    return s?.gaps.find((g) => g.id === gapId)?.options ?? [];
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
      for (const g of s.gaps) if (!this.getValue(s.id, g.id)) return false;
    }
    return true;
  }

  isCorrect(sentenceId: string, gapId: string): boolean {
    if (!this.showResult()) return false;
    const s = this.data.sentences.find((x) => x.id === sentenceId);
    const g = s?.gaps.find((x) => x.id === gapId);
    return g ? this.getValue(sentenceId, gapId) === g.correct : false;
  }

  isWrong(sentenceId: string, gapId: string): boolean {
    return this.showResult() && !this.isCorrect(sentenceId, gapId);
  }

  submit() {
    if (!this.allFilled()) return;
    this.submitted.set(true);
    const responses: FillGapResponse[] = this.data.sentences.map((s) => ({
      sentenceId: s.id,
      gaps: s.gaps.map((g) => ({ gapId: g.id, value: this.getValue(s.id, g.id) })),
    }));
    this.submitAnswer.emit({ type: 'Dropdown', responses });
  }
}

import { Component, EventEmitter, Input, OnInit, Output, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  MatchingBlockData,
  MatchingAnswer,
  MatchingAnswerPair,
  LessonBlockAttemptDto,
} from '../../models';

@Component({
  selector: 'app-matching-viewer',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (data.instruction) { <p class="instruction">{{ data.instruction }}</p> }

    <div class="pairs">
      @for (l of data.leftItems; track l.id) {
        <div class="pair"
          [class.pair--correct]="showResult() && isPairCorrect(l.id)"
          [class.pair--wrong]="showResult() && !isPairCorrect(l.id)"
        >
          <div class="left">{{ l.text }}</div>
          <div class="arrow">→</div>
          <select
            class="right"
            [disabled]="submitted()"
            [ngModel]="selection().get(l.id) || ''"
            (ngModelChange)="setPair(l.id, $event)"
          >
            <option value="">...</option>
            @for (r of shuffledRight(); track r.id) {
              <option [value]="r.id">{{ r.text }}</option>
            }
          </select>
        </div>
      }
    </div>

    @if (!submitted()) {
      <button type="button" class="submit" [disabled]="!allMatched()" (click)="submit()">Проверить</button>
    }
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .instruction { margin: 0; color: #64748B; font-size: 0.875rem; }
      .pairs { display: flex; flex-direction: column; gap: 8px; }
      .pair {
        display: grid; grid-template-columns: 1fr 32px 1fr; align-items: center; gap: 8px;
        padding: 10px 14px; background: #fff; border: 1px solid #E2E8F0; border-radius: 10px;
      }
      .pair--correct { border-color: #10B981; background: #DCFCE7; }
      .pair--wrong { border-color: #EF4444; background: #FEE2E2; }
      .arrow { text-align: center; color: #64748B; }
      .right { padding: 6px 10px; border: 1px solid #CBD5E1; border-radius: 6px; background: #F8FAFC; font: inherit; }
      .submit {
        align-self: flex-start; padding: 8px 20px; border: none;
        background: #4F46E5; color: #fff; border-radius: 8px; cursor: pointer;
        font-weight: 600; font-size: 0.875rem;
      }
      .submit:disabled { background: #CBD5E1; cursor: not-allowed; }
    `,
  ],
})
export class MatchingViewerComponent implements OnInit {
  @Input({ required: true }) data!: MatchingBlockData;
  @Input() attempt: LessonBlockAttemptDto | null = null;
  @Input() showFeedback = true;
  @Output() submitAnswer = new EventEmitter<MatchingAnswer>();

  selection = signal<Map<string, string>>(new Map());
  submitted = signal(false);
  shuffledRight = signal<typeof this.data.rightItems>([]);

  showResult = computed(() => this.submitted() && this.showFeedback);

  ngOnInit() {
    this.shuffledRight.set([...this.data.rightItems].sort(() => Math.random() - 0.5));

    if (this.attempt?.answers && this.attempt.answers.type === 'Matching') {
      const map = new Map<string, string>();
      for (const p of this.attempt.answers.pairs) map.set(p.leftId, p.rightId);
      this.selection.set(map);
      this.submitted.set(true);
    }
  }

  setPair(leftId: string, rightId: string) {
    const map = new Map(this.selection());
    map.set(leftId, rightId);
    this.selection.set(map);
  }

  allMatched(): boolean {
    return this.data.leftItems.every((l) => !!this.selection().get(l.id));
  }

  isPairCorrect(leftId: string): boolean {
    const given = this.selection().get(leftId);
    const correct = this.data.correctPairs.find((p) => p.leftId === leftId)?.rightId;
    return given === correct;
  }

  submit() {
    if (!this.allMatched()) return;
    this.submitted.set(true);
    const pairs: MatchingAnswerPair[] = Array.from(this.selection().entries()).map(
      ([leftId, rightId]) => ({ leftId, rightId }),
    );
    this.submitAnswer.emit({ type: 'Matching', pairs });
  }
}

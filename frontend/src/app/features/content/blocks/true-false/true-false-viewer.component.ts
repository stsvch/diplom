import { Component, EventEmitter, Input, OnInit, Output, signal, computed } from '@angular/core';
import {
  TrueFalseBlockData,
  TrueFalseAnswer,
  TrueFalseResponse,
  LessonBlockAttemptDto,
} from '../../models';

@Component({
  selector: 'app-true-false-viewer',
  standalone: true,
  template: `
    @if (data.instruction) { <p class="instruction">{{ data.instruction }}</p> }

    <div class="statements">
      @for (s of data.statements; track s.id) {
        <div class="statement"
          [class.statement--correct]="showResult() && isCorrect(s.id, s.isTrue)"
          [class.statement--wrong]="showResult() && !isCorrect(s.id, s.isTrue)"
        >
          <span class="text">{{ s.text }}</span>
          <div class="buttons">
            <button
              type="button"
              [class.active]="getAnswer(s.id) === true"
              [disabled]="submitted()"
              (click)="setAnswer(s.id, true)"
            >✓ Верно</button>
            <button
              type="button"
              [class.active]="getAnswer(s.id) === false"
              [disabled]="submitted()"
              (click)="setAnswer(s.id, false)"
            >✗ Неверно</button>
          </div>
        </div>
      }
    </div>

    @if (!submitted()) {
      <button type="button" class="submit" [disabled]="!allAnswered()" (click)="submit()">Проверить</button>
    }
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .instruction { margin: 0; color: #64748B; font-size: 0.875rem; }
      .statements { display: flex; flex-direction: column; gap: 8px; }
      .statement {
        display: flex; align-items: center; gap: 16px; padding: 12px 16px;
        border: 1px solid #E2E8F0; border-radius: 10px; background: #fff;
      }
      .statement--correct { border-color: #10B981; background: #DCFCE7; }
      .statement--wrong { border-color: #EF4444; background: #FEE2E2; }
      .text { flex: 1; font-size: 0.9375rem; color: #0F172A; }
      .buttons { display: flex; gap: 4px; flex-shrink: 0; }
      .buttons button {
        padding: 6px 12px; border: 1px solid #E2E8F0; background: #fff;
        border-radius: 8px; cursor: pointer; font-size: 0.8125rem; color: #64748B;
      }
      .buttons button.active { background: #4F46E5; border-color: #4F46E5; color: #fff; }
      .buttons button:disabled { opacity: 0.6; cursor: not-allowed; }
      .submit {
        align-self: flex-start; padding: 8px 20px; border: none;
        background: #4F46E5; color: #fff; border-radius: 8px; cursor: pointer;
        font-weight: 600; font-size: 0.875rem;
      }
      .submit:disabled { background: #CBD5E1; cursor: not-allowed; }
    `,
  ],
})
export class TrueFalseViewerComponent implements OnInit {
  @Input({ required: true }) data!: TrueFalseBlockData;
  @Input() attempt: LessonBlockAttemptDto | null = null;
  @Input() showFeedback = true;
  @Output() submitAnswer = new EventEmitter<TrueFalseAnswer>();

  responses = signal<Map<string, boolean>>(new Map());
  submitted = signal(false);

  showResult = computed(() => this.submitted() && this.showFeedback);

  ngOnInit() {
    if (this.attempt?.answers && 'responses' in this.attempt.answers) {
      const map = new Map<string, boolean>();
      for (const r of (this.attempt.answers as TrueFalseAnswer).responses) {
        map.set(r.statementId, r.answer);
      }
      this.responses.set(map);
      this.submitted.set(true);
    }
  }

  getAnswer(id: string): boolean | undefined {
    return this.responses().get(id);
  }

  setAnswer(id: string, value: boolean) {
    if (this.submitted()) return;
    const map = new Map(this.responses());
    map.set(id, value);
    this.responses.set(map);
  }

  allAnswered(): boolean {
    return this.data.statements.every((s) => this.responses().has(s.id));
  }

  isCorrect(id: string, expected: boolean): boolean {
    return this.responses().get(id) === expected;
  }

  submit() {
    if (!this.allAnswered()) return;
    this.submitted.set(true);
    const responses: TrueFalseResponse[] = Array.from(this.responses().entries()).map(
      ([statementId, answer]) => ({ statementId, answer }),
    );
    this.submitAnswer.emit({ type: 'TrueFalse', responses });
  }
}

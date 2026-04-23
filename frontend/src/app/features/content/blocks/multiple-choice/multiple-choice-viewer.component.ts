import { Component, EventEmitter, Input, OnInit, Output, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  MultipleChoiceBlockData,
  MultipleChoiceAnswer,
  LessonBlockAttemptDto,
} from '../../models';

@Component({
  selector: 'app-multiple-choice-viewer',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (data.instruction) { <p class="instruction">{{ data.instruction }}</p> }
    <h3 class="question">{{ data.question }}</h3>

    <div class="options">
      @for (opt of data.options; track opt.id) {
        <label
          class="option"
          [class.option--selected]="isSelected(opt.id)"
          [class.option--correct]="showResult() && opt.isCorrect"
          [class.option--wrong]="showResult() && isSelected(opt.id) && !opt.isCorrect"
        >
          <input
            type="checkbox"
            [checked]="isSelected(opt.id)"
            (change)="toggle(opt.id)"
            [disabled]="submitted()"
          />
          <span>{{ opt.text }}</span>
        </label>
      }
    </div>

    @if (!submitted()) {
      <button type="button" class="submit" [disabled]="selected().length === 0" (click)="submit()">
        Проверить
      </button>
    }
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .instruction { margin: 0; color: #64748B; font-size: 0.875rem; }
      .question { margin: 0; font-size: 1.125rem; font-weight: 600; color: #0F172A; }
      .options { display: flex; flex-direction: column; gap: 8px; }
      .option {
        display: flex; align-items: center; gap: 12px; padding: 12px 16px;
        border: 1px solid #E2E8F0; border-radius: 10px; cursor: pointer;
        background: #fff; transition: all 0.15s ease;
      }
      .option:hover:not(:has(input:disabled)) { border-color: #4F46E5; background: #EEF2FF; }
      .option input { width: 18px; height: 18px; accent-color: #4F46E5; }
      .option span { flex: 1; }
      .option--selected { border-color: #4F46E5; background: #EEF2FF; }
      .option--correct { border-color: #10B981; background: #DCFCE7; }
      .option--wrong { border-color: #EF4444; background: #FEE2E2; }
      .submit {
        align-self: flex-start; padding: 8px 20px; border: none;
        background: #4F46E5; color: #fff; border-radius: 8px; cursor: pointer;
        font-weight: 600; font-size: 0.875rem;
      }
      .submit:disabled { background: #CBD5E1; cursor: not-allowed; }
      .submit:hover:not(:disabled) { background: #4338CA; }
    `,
  ],
})
export class MultipleChoiceViewerComponent implements OnInit {
  @Input({ required: true }) data!: MultipleChoiceBlockData;
  @Input() attempt: LessonBlockAttemptDto | null = null;
  @Input() showFeedback = true;
  @Output() submitAnswer = new EventEmitter<MultipleChoiceAnswer>();

  selected = signal<string[]>([]);
  submitted = signal(false);

  showResult = computed(() => this.submitted() && this.showFeedback);

  ngOnInit() {
    if (this.attempt?.answers && 'selectedOptionIds' in this.attempt.answers) {
      this.selected.set([...this.attempt.answers.selectedOptionIds]);
      this.submitted.set(true);
    }
  }

  isSelected(id: string): boolean {
    return this.selected().includes(id);
  }

  toggle(id: string) {
    if (this.submitted()) return;
    const arr = this.selected();
    this.selected.set(arr.includes(id) ? arr.filter((x) => x !== id) : [...arr, id]);
  }

  submit() {
    if (this.selected().length === 0) return;
    this.submitted.set(true);
    this.submitAnswer.emit({ type: 'MultipleChoice', selectedOptionIds: this.selected() });
  }
}

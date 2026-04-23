import { Component, EventEmitter, Input, OnInit, Output, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  SingleChoiceBlockData,
  SingleChoiceAnswer,
  LessonBlockAttemptDto,
  ChoiceOption,
} from '../../models';

@Component({
  selector: 'app-single-choice-viewer',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (data.instruction) {
      <p class="instruction">{{ data.instruction }}</p>
    }
    <h3 class="question">{{ data.question }}</h3>

    <div class="options">
      @for (opt of options(); track opt.id) {
        <label
          class="option"
          [class.option--selected]="selected() === opt.id"
          [class.option--correct]="showResult() && opt.isCorrect"
          [class.option--wrong]="showResult() && selected() === opt.id && !opt.isCorrect"
        >
          <input
            type="radio"
            name="single-choice-{{ data.question }}"
            [value]="opt.id"
            [(ngModel)]="selectedModel"
            [disabled]="submitted()"
          />
          <span>{{ opt.text }}</span>
        </label>
      }
    </div>

    @if (!submitted()) {
      <button type="button" class="submit" [disabled]="!selected()" (click)="submit()">
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
        transition: all 0.15s ease; background: #fff;
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
export class SingleChoiceViewerComponent implements OnInit {
  @Input({ required: true }) data!: SingleChoiceBlockData;
  @Input() attempt: LessonBlockAttemptDto | null = null;
  @Input() showFeedback = true;

  @Output() submitAnswer = new EventEmitter<SingleChoiceAnswer>();

  selected = signal<string>('');
  submitted = signal(false);

  get selectedModel() {
    return this.selected();
  }
  set selectedModel(v: string) {
    this.selected.set(v);
  }

  options = computed<ChoiceOption[]>(() => this.data.options);

  showResult = computed(() => this.submitted() && this.showFeedback);

  ngOnInit() {
    if (this.attempt?.answers && 'selectedOptionId' in this.attempt.answers) {
      this.selected.set(this.attempt.answers.selectedOptionId);
      this.submitted.set(true);
    }
  }

  submit() {
    if (!this.selected()) return;
    this.submitted.set(true);
    this.submitAnswer.emit({ type: 'SingleChoice', selectedOptionId: this.selected() });
  }
}

import { Component, EventEmitter, Input, OnInit, Output, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  OpenTextBlockData,
  OpenTextAnswer,
  LessonBlockAttemptDto,
} from '../../models';

@Component({
  selector: 'app-open-text-viewer',
  standalone: true,
  imports: [FormsModule],
  template: `
    <p class="instruction">{{ data.instruction }}</p>
    @if (data.prompt) { <p class="prompt">{{ data.prompt }}</p> }

    @if (data.helperWords.length > 0) {
      <div class="helpers">
        @for (w of data.helperWords; track w) {
          <span class="chip">{{ w }}</span>
        }
      </div>
    }

    <textarea
      class="textarea"
      rows="8"
      [placeholder]="textareaPlaceholder"
      [disabled]="submitted()"
      [ngModel]="text()"
      (ngModelChange)="text.set($event)"
    ></textarea>

    <div class="meta">
      <span [class.over]="overLimit()">{{ length() }} {{ unitLabel }}</span>
      @if (data.minLength) { <span>· мин {{ data.minLength }}</span> }
      @if (data.maxLength) { <span>· макс {{ data.maxLength }}</span> }
    </div>

    @if (!submitted()) {
      <button type="button" class="submit" [disabled]="!canSubmit()" (click)="submit()">Отправить</button>
    } @else {
      <p class="review">Отправлено на проверку преподавателю.</p>
    }
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .instruction { margin: 0; font-size: 1rem; font-weight: 600; color: #0F172A; }
      .prompt { margin: 0; color: #4B5563; font-size: 0.9375rem; }
      .helpers { display: flex; flex-wrap: wrap; gap: 6px; }
      .chip { padding: 4px 10px; background: #EEF2FF; color: #4F46E5; border-radius: 999px; font-size: 0.8125rem; }
      .textarea {
        width: 100%; padding: 12px; border: 1px solid #E2E8F0; border-radius: 10px;
        background: #F8FAFC; font: inherit; font-size: 0.9375rem; resize: vertical; outline: none;
      }
      .textarea:focus { border-color: #4F46E5; background: #fff; }
      .meta { display: flex; gap: 8px; font-size: 0.75rem; color: #64748B; }
      .meta .over { color: #EF4444; font-weight: 600; }
      .submit {
        align-self: flex-start; padding: 8px 20px; border: none;
        background: #4F46E5; color: #fff; border-radius: 8px; cursor: pointer;
        font-weight: 600; font-size: 0.875rem;
      }
      .submit:disabled { background: #CBD5E1; cursor: not-allowed; }
      .review { padding: 12px; background: #FEF3C7; color: #92400E; border-radius: 8px; margin: 0; font-size: 0.875rem; }
    `,
  ],
})
export class OpenTextViewerComponent implements OnInit {
  @Input({ required: true }) data!: OpenTextBlockData;
  @Input() attempt: LessonBlockAttemptDto | null = null;
  @Input() showFeedback = true;
  @Output() submitAnswer = new EventEmitter<OpenTextAnswer>();

  text = signal('');
  submitted = signal(false);

  length = computed(() => {
    const v = this.text();
    return this.data.unit === 'Words' ? v.trim().split(/\s+/).filter(Boolean).length : v.length;
  });

  overLimit = computed(() => !!this.data.maxLength && this.length() > this.data.maxLength);

  canSubmit = computed(() => {
    const l = this.length();
    if (this.data.minLength && l < this.data.minLength) return false;
    if (this.data.maxLength && l > this.data.maxLength) return false;
    return l > 0;
  });

  get unitLabel(): string {
    return this.data.unit === 'Words' ? 'слов' : 'символов';
  }

  get textareaPlaceholder(): string {
    return 'Ваш ответ...';
  }

  ngOnInit() {
    if (this.attempt?.answers && this.attempt.answers.type === 'OpenText') {
      this.text.set(this.attempt.answers.text);
      this.submitted.set(true);
    }
  }

  submit() {
    if (!this.canSubmit()) return;
    this.submitted.set(true);
    this.submitAnswer.emit({ type: 'OpenText', text: this.text() });
  }
}

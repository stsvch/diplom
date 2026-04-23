import { Component, EventEmitter, Input, OnInit, Output, signal, computed } from '@angular/core';
import {
  ReorderBlockData,
  ReorderAnswer,
  LessonBlockAttemptDto,
} from '../../models';

@Component({
  selector: 'app-reorder-viewer',
  standalone: true,
  template: `
    @if (data.instruction) { <p class="instruction">{{ data.instruction }}</p> }

    <ol class="items">
      @for (id of order(); track id; let i = $index) {
        <li
          class="item"
          [class.item--correct]="showResult() && isItemCorrect(i, id)"
          [class.item--wrong]="showResult() && !isItemCorrect(i, id)"
        >
          <span class="num">{{ i + 1 }}</span>
          <span class="text">{{ getText(id) }}</span>
          @if (!submitted()) {
            <div class="controls">
              <button type="button" (click)="move(i, -1)" [disabled]="i === 0">↑</button>
              <button type="button" (click)="move(i, 1)" [disabled]="i === order().length - 1">↓</button>
            </div>
          }
        </li>
      }
    </ol>

    @if (!submitted()) {
      <button type="button" class="submit" (click)="submit()">Проверить</button>
    }
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .instruction { margin: 0; color: #64748B; font-size: 0.875rem; }
      .items { margin: 0; padding: 0; list-style: none; display: flex; flex-direction: column; gap: 6px; }
      .item {
        display: flex; align-items: center; gap: 12px; padding: 10px 14px;
        background: #fff; border: 1px solid #E2E8F0; border-radius: 10px;
      }
      .item--correct { border-color: #10B981; background: #DCFCE7; }
      .item--wrong { border-color: #EF4444; background: #FEE2E2; }
      .num {
        width: 24px; height: 24px; border-radius: 999px; background: #4F46E5; color: #fff;
        display: inline-flex; align-items: center; justify-content: center; font-size: 0.75rem; font-weight: 700;
      }
      .text { flex: 1; }
      .controls { display: flex; gap: 2px; }
      .controls button {
        width: 28px; height: 28px; border: 1px solid #E2E8F0; background: #fff;
        border-radius: 6px; cursor: pointer; color: #64748B;
      }
      .controls button:disabled { opacity: 0.3; cursor: not-allowed; }
      .controls button:hover:not(:disabled) { background: #EEF2FF; color: #4F46E5; }
      .submit {
        align-self: flex-start; padding: 8px 20px; border: none;
        background: #4F46E5; color: #fff; border-radius: 8px; cursor: pointer;
        font-weight: 600; font-size: 0.875rem;
      }
    `,
  ],
})
export class ReorderViewerComponent implements OnInit {
  @Input({ required: true }) data!: ReorderBlockData;
  @Input() attempt: LessonBlockAttemptDto | null = null;
  @Input() showFeedback = true;
  @Output() submitAnswer = new EventEmitter<ReorderAnswer>();

  order = signal<string[]>([]);
  submitted = signal(false);

  showResult = computed(() => this.submitted() && this.showFeedback);

  ngOnInit() {
    if (this.attempt?.answers && this.attempt.answers.type === 'Reorder') {
      this.order.set([...this.attempt.answers.order]);
      this.submitted.set(true);
    } else {
      // Перемешиваем порядок для студента
      const shuffled = [...this.data.items.map((i) => i.id)].sort(() => Math.random() - 0.5);
      this.order.set(shuffled);
    }
  }

  getText(id: string): string {
    return this.data.items.find((i) => i.id === id)?.text ?? '';
  }

  move(idx: number, dir: -1 | 1) {
    if (this.submitted()) return;
    const arr = [...this.order()];
    const j = idx + dir;
    if (j < 0 || j >= arr.length) return;
    [arr[idx], arr[j]] = [arr[j], arr[idx]];
    this.order.set(arr);
  }

  isItemCorrect(index: number, id: string): boolean {
    return this.data.correctOrder[index] === id;
  }

  submit() {
    this.submitted.set(true);
    this.submitAnswer.emit({ type: 'Reorder', order: this.order() });
  }
}

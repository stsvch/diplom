import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { MatchingBlockData } from '../../models';

@Component({
  selector: 'app-matching-editor',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  template: `
    <input
      type="text" class="input" placeholder="Инструкция (опц.)"
      [ngModel]="data.instruction"
      (ngModelChange)="update({ instruction: $event })"
    />

    <p class="hint">Каждая строка — пара. Левая и правая колонки студенту перемешаются независимо.</p>

    <div class="pairs">
      @for (pair of data.correctPairs; track pair.leftId; let i = $index) {
        <div class="pair">
          <input
            type="text" class="input"
            placeholder="Левый элемент"
            [ngModel]="getLeftText(pair.leftId)"
            (ngModelChange)="setLeftText(i, $event)"
          />
          <div class="arrow">⇄</div>
          <input
            type="text" class="input"
            placeholder="Правый элемент"
            [ngModel]="getRightText(pair.rightId)"
            (ngModelChange)="setRightText(i, $event)"
          />
          <button type="button" class="remove" (click)="removePair(i)" [disabled]="data.correctPairs.length <= 1">
            <lucide-icon name="x" size="14"></lucide-icon>
          </button>
        </div>
      }
      <button type="button" class="add" (click)="addPair()">
        <lucide-icon name="plus" size="14"></lucide-icon>
        Добавить пару
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
      .hint { margin: 0; font-size: 0.75rem; color: #64748B; }
      .pairs { display: flex; flex-direction: column; gap: 8px; }
      .pair { display: grid; grid-template-columns: 1fr 32px 1fr 24px; align-items: center; gap: 8px; }
      .arrow { text-align: center; color: #64748B; font-size: 1.25rem; }
      .remove {
        width: 24px; height: 24px; border: none; background: transparent; color: #64748B;
        cursor: pointer; border-radius: 4px; display: inline-flex; align-items: center; justify-content: center;
      }
      .remove:hover:not(:disabled) { background: #FEE2E2; color: #EF4444; }
      .remove:disabled { opacity: 0.3; cursor: not-allowed; }
      .add {
        align-self: flex-start; display: inline-flex; align-items: center; gap: 4px;
        padding: 6px 12px; border: 1px dashed #CBD5E1; background: transparent;
        border-radius: 8px; cursor: pointer; color: #64748B; font-size: 0.875rem;
      }
      .add:hover { color: #4F46E5; border-color: #4F46E5; }
    `,
  ],
})
export class MatchingEditorComponent {
  @Input({ required: true }) data!: MatchingBlockData;
  @Output() dataChange = new EventEmitter<MatchingBlockData>();

  update(patch: Partial<MatchingBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }

  getLeftText(id: string): string {
    return this.data.leftItems.find((i) => i.id === id)?.text ?? '';
  }
  getRightText(id: string): string {
    return this.data.rightItems.find((i) => i.id === id)?.text ?? '';
  }

  setLeftText(pairIndex: number, text: string) {
    const pair = this.data.correctPairs[pairIndex];
    const leftItems = this.data.leftItems.map((i) => (i.id === pair.leftId ? { ...i, text } : i));
    this.update({ leftItems });
  }

  setRightText(pairIndex: number, text: string) {
    const pair = this.data.correctPairs[pairIndex];
    const rightItems = this.data.rightItems.map((i) => (i.id === pair.rightId ? { ...i, text } : i));
    this.update({ rightItems });
  }

  addPair() {
    const leftIds = new Set(this.data.leftItems.map((i) => i.id));
    const rightIds = new Set(this.data.rightItems.map((i) => i.id));
    let n = 1;
    while (leftIds.has(`l${n}`)) n++;
    const leftId = `l${n}`;
    let m = 1;
    while (rightIds.has(`r${m}`)) m++;
    const rightId = `r${m}`;
    this.update({
      leftItems: [...this.data.leftItems, { id: leftId, text: '' }],
      rightItems: [...this.data.rightItems, { id: rightId, text: '' }],
      correctPairs: [...this.data.correctPairs, { leftId, rightId }],
    });
  }

  removePair(i: number) {
    if (this.data.correctPairs.length <= 1) return;
    const pair = this.data.correctPairs[i];
    this.update({
      leftItems: this.data.leftItems.filter((x) => x.id !== pair.leftId),
      rightItems: this.data.rightItems.filter((x) => x.id !== pair.rightId),
      correctPairs: this.data.correctPairs.filter((_, idx) => idx !== i),
    });
  }
}

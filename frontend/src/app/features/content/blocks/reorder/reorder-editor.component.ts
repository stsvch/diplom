import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { ReorderBlockData, ReorderItem } from '../../models';

@Component({
  selector: 'app-reorder-editor',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  template: `
    <input
      type="text" class="input" placeholder="Инструкция (опц.)"
      [ngModel]="data.instruction"
      (ngModelChange)="update({ instruction: $event })"
    />

    <p class="hint">Введите пункты в <b>правильном</b> порядке — студенту они будут показаны в случайном.</p>

    <div class="items">
      @for (item of data.items; track item.id; let i = $index) {
        <div class="item">
          <span class="num">{{ i + 1 }}</span>
          <input
            type="text" class="input"
            placeholder="Пункт {{ i + 1 }}"
            [ngModel]="item.text"
            (ngModelChange)="updateItem(i, { text: $event })"
          />
          <div class="move">
            <button type="button" (click)="moveItem(i, -1)" [disabled]="i === 0">↑</button>
            <button type="button" (click)="moveItem(i, 1)" [disabled]="i === data.items.length - 1">↓</button>
          </div>
          <button type="button" class="remove" (click)="removeItem(i)" [disabled]="data.items.length <= 2">
            <lucide-icon name="x" size="14"></lucide-icon>
          </button>
        </div>
      }
      <button type="button" class="add" (click)="addItem()">
        <lucide-icon name="plus" size="14"></lucide-icon>
        Добавить пункт
      </button>
    </div>

    <label class="toggle">
      <input type="checkbox" [checked]="data.allOrNothing" (change)="update({ allOrNothing: $any($event.target).checked })" />
      <span>Всё или ничего (иначе баллы по позициям)</span>
    </label>
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
      .items { display: flex; flex-direction: column; gap: 6px; }
      .item { display: flex; align-items: center; gap: 8px; }
      .num {
        width: 24px; height: 24px; border-radius: 999px; background: #4F46E5; color: #fff;
        display: inline-flex; align-items: center; justify-content: center; font-size: 0.75rem; font-weight: 700;
      }
      .move { display: flex; gap: 2px; }
      .move button {
        width: 24px; height: 24px; border: 1px solid #E2E8F0; background: #fff;
        border-radius: 4px; cursor: pointer; color: #64748B;
      }
      .move button:disabled { opacity: 0.3; cursor: not-allowed; }
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
      .toggle { display: inline-flex; align-items: center; gap: 8px; font-size: 0.875rem; color: #4B5563; }
    `,
  ],
})
export class ReorderEditorComponent {
  @Input({ required: true }) data!: ReorderBlockData;
  @Output() dataChange = new EventEmitter<ReorderBlockData>();

  update(patch: Partial<ReorderBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }

  updateItem(i: number, patch: Partial<ReorderItem>) {
    const items = this.data.items.map((it, idx) => (idx === i ? { ...it, ...patch } : it));
    this.update({ items });
  }

  moveItem(i: number, dir: -1 | 1) {
    const items = [...this.data.items];
    const j = i + dir;
    if (j < 0 || j >= items.length) return;
    [items[i], items[j]] = [items[j], items[i]];
    const correctOrder = items.map((it) => it.id);
    this.update({ items, correctOrder });
  }

  addItem() {
    const used = new Set(this.data.items.map((i) => i.id));
    let n = 1;
    while (used.has(`i${n}`)) n++;
    const id = `i${n}`;
    const items = [...this.data.items, { id, text: '' }];
    this.update({ items, correctOrder: [...this.data.correctOrder, id] });
  }

  removeItem(i: number) {
    if (this.data.items.length <= 2) return;
    const removedId = this.data.items[i].id;
    const items = this.data.items.filter((_, idx) => idx !== i);
    const correctOrder = this.data.correctOrder.filter((id) => id !== removedId);
    this.update({ items, correctOrder });
  }
}

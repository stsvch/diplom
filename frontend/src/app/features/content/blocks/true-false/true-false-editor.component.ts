import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { TrueFalseBlockData, TrueFalseStatement } from '../../models';

@Component({
  selector: 'app-true-false-editor',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  template: `
    <input
      type="text" class="input" placeholder="Инструкция (опц.)"
      [ngModel]="data.instruction"
      (ngModelChange)="update({ instruction: $event })"
    />

    <div class="list">
      @for (s of data.statements; track s.id; let i = $index) {
        <div class="row">
          <input
            type="text" class="input" placeholder="Утверждение"
            [ngModel]="s.text"
            (ngModelChange)="updateStmt(i, { text: $event })"
          />
          <div class="toggle">
            <button type="button" [class.active]="s.isTrue" (click)="updateStmt(i, { isTrue: true })">✓ Верно</button>
            <button type="button" [class.active]="!s.isTrue" (click)="updateStmt(i, { isTrue: false })">✗ Неверно</button>
          </div>
          <button type="button" class="remove" (click)="remove(i)" [disabled]="data.statements.length <= 1">
            <lucide-icon name="x" size="14"></lucide-icon>
          </button>
        </div>
      }
      <button type="button" class="add" (click)="add()">
        <lucide-icon name="plus" size="14"></lucide-icon>
        Добавить утверждение
      </button>
    </div>
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .input {
        padding: 8px 12px; border: 1px solid #E2E8F0; border-radius: 8px;
        background: #F8FAFC; font: inherit; font-size: 0.875rem; outline: none; flex: 1;
      }
      .input:focus { border-color: #4F46E5; background: #fff; }
      .list { display: flex; flex-direction: column; gap: 8px; }
      .row { display: flex; gap: 8px; align-items: center; }
      .toggle { display: flex; gap: 2px; }
      .toggle button {
        padding: 6px 10px; border: 1px solid #E2E8F0; background: #fff;
        border-radius: 8px; cursor: pointer; font-size: 0.75rem; color: #64748B;
      }
      .toggle button.active { background: #4F46E5; color: #fff; border-color: #4F46E5; }
      .remove {
        width: 24px; height: 24px; border: none; background: transparent; color: #64748B;
        cursor: pointer; border-radius: 6px; display: inline-flex; align-items: center; justify-content: center;
      }
      .remove:hover:not(:disabled) { background: #FEE2E2; color: #EF4444; }
      .remove:disabled { opacity: 0.3; cursor: not-allowed; }
      .add {
        align-self: flex-start; display: inline-flex; align-items: center; gap: 6px;
        padding: 6px 12px; border: 1px dashed #CBD5E1; background: transparent;
        border-radius: 8px; cursor: pointer; color: #64748B; font-size: 0.875rem;
      }
      .add:hover { color: #4F46E5; border-color: #4F46E5; }
    `,
  ],
})
export class TrueFalseEditorComponent {
  @Input({ required: true }) data!: TrueFalseBlockData;
  @Output() dataChange = new EventEmitter<TrueFalseBlockData>();

  update(patch: Partial<TrueFalseBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }

  updateStmt(i: number, patch: Partial<TrueFalseStatement>) {
    const statements = this.data.statements.map((s, idx) => (idx === i ? { ...s, ...patch } : s));
    this.update({ statements });
  }

  add() {
    const used = new Set(this.data.statements.map((s) => s.id));
    let n = 1;
    while (used.has(`s${n}`)) n++;
    this.update({
      statements: [...this.data.statements, { id: `s${n}`, text: '', isTrue: true }],
    });
  }

  remove(i: number) {
    if (this.data.statements.length <= 1) return;
    this.update({ statements: this.data.statements.filter((_, idx) => idx !== i) });
  }
}

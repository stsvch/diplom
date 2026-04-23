import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { MultipleChoiceBlockData, ChoiceOption } from '../../models';

@Component({
  selector: 'app-multiple-choice-editor',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  template: `
    <input
      type="text" class="input" placeholder="Инструкция (опц.)"
      [ngModel]="data.instruction"
      (ngModelChange)="update({ instruction: $event })"
    />
    <input
      type="text" class="input input--question" placeholder="Вопрос"
      [ngModel]="data.question"
      (ngModelChange)="update({ question: $event })"
    />

    <label class="toggle">
      <input type="checkbox" [checked]="data.partialCredit" (change)="update({ partialCredit: $any($event.target).checked })" />
      <span>Частичные баллы (доля правильных)</span>
    </label>

    <div class="options">
      @for (opt of data.options; track opt.id; let i = $index) {
        <div class="option">
          <label class="checkbox">
            <input type="checkbox" [checked]="opt.isCorrect" (change)="toggleCorrect(i)" />
            <span class="checkbox__visual"><lucide-icon name="check" size="14"></lucide-icon></span>
          </label>
          <input type="text" class="input input--option" placeholder="Вариант {{ i + 1 }}"
            [ngModel]="opt.text"
            (ngModelChange)="updateOption(i, { text: $event })"
          />
          <button type="button" class="remove" (click)="removeOption(i)" [disabled]="data.options.length <= 2">
            <lucide-icon name="x" size="14"></lucide-icon>
          </button>
        </div>
      }
      <button type="button" class="add" (click)="addOption()">
        <lucide-icon name="plus" size="14"></lucide-icon>
        Добавить вариант
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
      .input--question { font-size: 1rem; font-weight: 600; }
      .input--option { flex: 1; }
      .toggle { display: inline-flex; align-items: center; gap: 8px; font-size: 0.875rem; color: #4B5563; }
      .options { display: flex; flex-direction: column; gap: 8px; }
      .option { display: flex; align-items: center; gap: 8px; }
      .checkbox { cursor: pointer; }
      .checkbox input { display: none; }
      .checkbox__visual {
        width: 20px; height: 20px; border: 2px solid #CBD5E1; border-radius: 6px;
        display: inline-flex; align-items: center; justify-content: center; color: transparent;
        transition: all 0.15s ease;
      }
      .checkbox input:checked + .checkbox__visual {
        background: #10B981; border-color: #10B981; color: #fff;
      }
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
export class MultipleChoiceEditorComponent {
  @Input({ required: true }) data!: MultipleChoiceBlockData;
  @Output() dataChange = new EventEmitter<MultipleChoiceBlockData>();

  update(patch: Partial<MultipleChoiceBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }

  updateOption(i: number, patch: Partial<ChoiceOption>) {
    const options = this.data.options.map((o, idx) => (idx === i ? { ...o, ...patch } : o));
    this.update({ options });
  }

  toggleCorrect(i: number) {
    const options = this.data.options.map((o, idx) => (idx === i ? { ...o, isCorrect: !o.isCorrect } : o));
    this.update({ options });
  }

  addOption() {
    const used = new Set(this.data.options.map((o) => o.id));
    let code = 97;
    while (used.has(String.fromCharCode(code))) code++;
    this.update({
      options: [...this.data.options, { id: String.fromCharCode(code), text: '', isCorrect: false }],
    });
  }

  removeOption(i: number) {
    if (this.data.options.length <= 2) return;
    this.update({ options: this.data.options.filter((_, idx) => idx !== i) });
  }
}

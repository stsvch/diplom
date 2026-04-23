import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { SingleChoiceBlockData, ChoiceOption } from '../../models';

@Component({
  selector: 'app-single-choice-editor',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  template: `
    <input
      type="text"
      class="input"
      placeholder="Инструкция (опц.)"
      [ngModel]="data.instruction"
      (ngModelChange)="update({ instruction: $event })"
    />
    <input
      type="text"
      class="input input--question"
      placeholder="Вопрос"
      [ngModel]="data.question"
      (ngModelChange)="update({ question: $event })"
    />

    <div class="options">
      @for (opt of data.options; track opt.id; let i = $index) {
        <div class="option">
          <label class="radio">
            <input
              type="radio"
              name="correct"
              [checked]="opt.isCorrect"
              (change)="markCorrect(i)"
            />
            <span class="radio__visual"></span>
          </label>
          <input
            type="text"
            class="input input--option"
            placeholder="Вариант {{ i + 1 }}"
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
      .options { display: flex; flex-direction: column; gap: 8px; }
      .option {
        display: flex; align-items: center; gap: 8px;
      }
      .radio { cursor: pointer; display: inline-flex; align-items: center; }
      .radio input { display: none; }
      .radio__visual {
        width: 20px; height: 20px; border-radius: 999px; border: 2px solid #CBD5E1;
        display: block; position: relative; transition: all 0.15s ease;
      }
      .radio input:checked + .radio__visual {
        border-color: #10B981; background: #10B981;
      }
      .radio input:checked + .radio__visual::after {
        content: ''; position: absolute; top: 3px; left: 3px;
        width: 10px; height: 10px; border-radius: 999px; background: #fff;
      }
      .input--option { flex: 1; }
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
export class SingleChoiceEditorComponent {
  @Input({ required: true }) data!: SingleChoiceBlockData;
  @Output() dataChange = new EventEmitter<SingleChoiceBlockData>();

  update(patch: Partial<SingleChoiceBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }

  updateOption(index: number, patch: Partial<ChoiceOption>) {
    const options = this.data.options.map((o, i) => (i === index ? { ...o, ...patch } : o));
    this.update({ options });
  }

  markCorrect(index: number) {
    const options = this.data.options.map((o, i) => ({ ...o, isCorrect: i === index }));
    this.update({ options });
  }

  addOption() {
    const id = this.nextId();
    this.update({
      options: [...this.data.options, { id, text: '', isCorrect: false }],
    });
  }

  removeOption(index: number) {
    if (this.data.options.length <= 2) return;
    const options = this.data.options.filter((_, i) => i !== index);
    if (!options.some((o) => o.isCorrect)) options[0].isCorrect = true;
    this.update({ options });
  }

  private nextId(): string {
    const used = new Set(this.data.options.map((o) => o.id));
    let code = 97; // 'a'
    while (used.has(String.fromCharCode(code))) code++;
    return String.fromCharCode(code);
  }
}

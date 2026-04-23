import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';

export interface CourseSettingsValue {
  deadline: string;
  hasCertificate: boolean;
  orderType: string;
  hasGrading: boolean;
}

@Component({
  selector: 'app-course-settings-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <form [formGroup]="form" class="settings-form">
      <div class="field">
        <label>Дедлайн</label>
        <input type="date" class="input" formControlName="deadline" />
        <span class="hint">Дата, до которой необходимо завершить курс (необязательно)</span>
      </div>

      <div class="field">
        <label>Порядок прохождения</label>
        <select class="input" formControlName="orderType">
          <option value="Sequential">Последовательный</option>
          <option value="Free">Свободный</option>
        </select>
        <span class="hint">«Последовательный» — следующий урок открывается после завершения предыдущего</span>
      </div>

      <div class="toggles">
        <label class="toggle">
          <input type="checkbox" formControlName="hasCertificate" />
          <span>
            <strong>Сертификат по завершении</strong>
            <em>Студенты получат сертификат после прохождения курса</em>
          </span>
        </label>
        <label class="toggle">
          <input type="checkbox" formControlName="hasGrading" />
          <span>
            <strong>Оценивание</strong>
            <em>Включить систему оценок для курса</em>
          </span>
        </label>
      </div>
    </form>
  `,
  styles: [
    `
      :host { display: block; }
      .settings-form { display: flex; flex-direction: column; gap: 16px; }
      .field { display: flex; flex-direction: column; gap: 4px; }
      .field label { font-size: 0.875rem; font-weight: 500; color: #4B5563; }
      .input {
        padding: 8px 12px; border: 1px solid #E2E8F0; border-radius: 8px;
        background: #F8FAFC; font: inherit; font-size: 0.875rem; outline: none;
      }
      .input:focus { border-color: #4F46E5; background: #fff; box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1); }
      .hint { font-size: 0.75rem; color: #64748B; }
      .toggles { display: flex; flex-direction: column; gap: 8px; }
      .toggle { display: flex; align-items: flex-start; gap: 10px; cursor: pointer; padding: 10px; border: 1px solid #E2E8F0; border-radius: 8px; background: #F8FAFC; }
      .toggle:hover { background: #fff; border-color: #4F46E5; }
      .toggle input { margin-top: 3px; }
      .toggle span { display: flex; flex-direction: column; gap: 2px; }
      .toggle strong { font-size: 0.875rem; color: #0F172A; font-weight: 600; }
      .toggle em { font-size: 0.75rem; color: #64748B; font-style: normal; }
    `,
  ],
})
export class CourseSettingsFormComponent {
  @Input() set value(v: Partial<CourseSettingsValue> | null | undefined) {
    if (v) this.form.patchValue(v, { emitEvent: false });
  }

  @Output() valueChange = new EventEmitter<CourseSettingsValue>();

  form: FormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      deadline: [''],
      hasCertificate: [false],
      orderType: ['Sequential'],
      hasGrading: [false],
    });
    this.form.valueChanges.subscribe((v) => this.valueChange.emit(v as CourseSettingsValue));
  }

  getValue(): CourseSettingsValue {
    return this.form.value as CourseSettingsValue;
  }
}

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BannerBlockData } from '../../models';

const PALETTE = [
  { label: 'Indigo', bg: '#4F46E5', fg: '#ffffff' },
  { label: 'Emerald', bg: '#10B981', fg: '#ffffff' },
  { label: 'Amber', bg: '#F59E0B', fg: '#ffffff' },
  { label: 'Rose', bg: '#F43F5E', fg: '#ffffff' },
  { label: 'Sky', bg: '#0EA5E9', fg: '#ffffff' },
  { label: 'Lime', bg: '#B0E86A', fg: '#1a1a1a' },
];

@Component({
  selector: 'app-banner-block-editor',
  standalone: true,
  imports: [FormsModule],
  template: `
    <input
      type="text"
      class="input input--title"
      placeholder="Заголовок баннера"
      [ngModel]="data.title"
      (ngModelChange)="update({ title: $event })"
    />

    <div class="row">
      <div class="palette">
        @for (c of palette; track c.label) {
          <button
            type="button"
            class="swatch"
            [class.active]="data.bgColor === c.bg"
            [style.backgroundColor]="c.bg"
            [attr.aria-label]="c.label"
            (click)="update({ bgColor: c.bg, textColor: c.fg })"
          ></button>
        }
      </div>
      <input
        type="color"
        class="color-picker"
        [ngModel]="data.bgColor || '#4F46E5'"
        (ngModelChange)="update({ bgColor: $event })"
      />
    </div>

    <input
      type="url"
      class="input"
      placeholder="URL изображения (опционально)"
      [ngModel]="data.imageUrl"
      (ngModelChange)="update({ imageUrl: $event })"
    />

    <div class="preview"
      [style.backgroundColor]="data.bgColor || '#4F46E5'"
      [style.color]="data.textColor || '#ffffff'"
    >
      {{ data.title || 'Превью баннера' }}
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
      .input--title { font-size: 1.125rem; font-weight: 600; }
      .row { display: flex; gap: 12px; align-items: center; }
      .palette { display: flex; gap: 6px; }
      .swatch {
        width: 28px; height: 28px; border-radius: 999px; border: 2px solid transparent;
        cursor: pointer;
      }
      .swatch.active { border-color: #0F172A; }
      .color-picker { width: 40px; height: 32px; border: 1px solid #E2E8F0; border-radius: 6px; cursor: pointer; }
      .preview {
        padding: 24px; border-radius: 12px; font-size: 1.5rem; font-weight: 700; min-height: 80px;
        display: flex; align-items: center;
      }
    `,
  ],
})
export class BannerBlockEditorComponent {
  @Input({ required: true }) data!: BannerBlockData;
  @Output() dataChange = new EventEmitter<BannerBlockData>();

  palette = PALETTE;

  update(patch: Partial<BannerBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }
}

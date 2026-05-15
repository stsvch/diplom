// text-block-viewer.component.ts
import { Component, Input } from '@angular/core';
import { RichTextViewerComponent } from '../../../../shared/components/rich-text-viewer/rich-text-viewer.component';
import { TextBlockData } from '../../models';

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-text-block-viewer',
  standalone: true,
  imports: [RichTextViewerComponent],
  template: `
    @if (hasContent()) {
      <app-rich-text-viewer [content]="data.html"></app-rich-text-viewer>
    } @else {
      <p class="empty">Текстовый блок без содержимого</p>
    }
  `,
  styles: [
    `
      :host { display: block; }
      .empty {
        margin: 0;
        padding: 8px 12px;
        font-size: 0.875rem;
        color: #94a3b8;
        font-style: italic;
        border-radius: 8px;
        background: #f8fafc;
        border: 1px dashed #e2e8f0;
      }
    `,
  ],
})
export class TextBlockViewerComponent {
  @Input({ required: true }) data!: TextBlockData;

  hasContent(): boolean {
    const html = this.data?.html ?? '';
    return html.replace(/<[^>]*>/g, '').trim().length > 0;
  }
}

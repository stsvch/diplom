// rich-text-viewer.component.ts
import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { renderMarkdown } from './markdown';

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-rich-text-viewer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './rich-text-viewer.component.html',
  styleUrl: './rich-text-viewer.component.scss',
})
export class RichTextViewerComponent {
  private readonly sanitizer = inject(DomSanitizer);

  private _safeContent: SafeHtml = '';

  @Input() set content(val: string) {
    const html = renderMarkdown(val ?? '');
    this._safeContent = this.sanitizer.bypassSecurityTrustHtml(html);
  }

  get safeContent(): SafeHtml {
    return this._safeContent;
  }
}

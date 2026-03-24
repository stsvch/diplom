import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

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
    this._safeContent = this.sanitizer.bypassSecurityTrustHtml(val ?? '');
  }

  get safeContent(): SafeHtml {
    return this._safeContent;
  }
}

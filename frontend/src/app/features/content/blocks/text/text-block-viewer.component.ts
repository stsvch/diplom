import { Component, Input } from '@angular/core';
import { RichTextViewerComponent } from '../../../../shared/components/rich-text-viewer/rich-text-viewer.component';
import { TextBlockData } from '../../models';

@Component({
  selector: 'app-text-block-viewer',
  standalone: true,
  imports: [RichTextViewerComponent],
  template: `<app-rich-text-viewer [content]="data.html"></app-rich-text-viewer>`,
})
export class TextBlockViewerComponent {
  @Input({ required: true }) data!: TextBlockData;
}

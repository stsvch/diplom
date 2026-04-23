import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RichTextEditorComponent } from '../../../../shared/components/rich-text-editor/rich-text-editor.component';
import { TextBlockData } from '../../models';

@Component({
  selector: 'app-text-block-editor',
  standalone: true,
  imports: [RichTextEditorComponent],
  template: `
    <app-rich-text-editor
      [content]="data.html"
      placeholder="Начните писать..."
      (contentChange)="onChange($event)"
    ></app-rich-text-editor>
  `,
})
export class TextBlockEditorComponent {
  @Input({ required: true }) data!: TextBlockData;
  @Output() dataChange = new EventEmitter<TextBlockData>();

  onChange(html: string) {
    this.dataChange.emit({ ...this.data, html });
  }
}

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ImageBlockData } from '../../models';
import { FileUploaderComponent } from '../../../../shared/components/file-uploader/file-uploader.component';
import { AttachmentDto } from '../../../../core/models/attachment.model';

@Component({
  selector: 'app-image-block-editor',
  standalone: true,
  imports: [FormsModule, FileUploaderComponent],
  template: `
    @if (!data.url) {
      <app-file-uploader
        accept="image/*"
        entityType="LessonBlock"
        [entityId]="blockId"
        (fileUploaded)="onUploaded($event)"
      ></app-file-uploader>
    } @else {
      <img [src]="data.url" [alt]="data.alt || ''" class="preview" />
      <button type="button" class="remove" (click)="update({ url: '' })">Удалить изображение</button>
    }

    <input
      type="text"
      class="input"
      placeholder="Описание (alt)"
      [ngModel]="data.alt"
      (ngModelChange)="update({ alt: $event })"
    />
    <input
      type="text"
      class="input"
      placeholder="Подпись"
      [ngModel]="data.caption"
      (ngModelChange)="update({ caption: $event })"
    />
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .input {
        padding: 8px 12px; border: 1px solid #E2E8F0; border-radius: 8px;
        background: #F8FAFC; font: inherit; font-size: 0.875rem; outline: none;
      }
      .input:focus { border-color: #4F46E5; background: #fff; }
      .preview { max-width: 100%; max-height: 320px; border-radius: 8px; }
      .remove { align-self: flex-start; padding: 4px 10px; border: 1px solid #E2E8F0;
                background: #fff; border-radius: 6px; cursor: pointer; font-size: 0.75rem; color: #EF4444; }
      .remove:hover { background: #FEE2E2; }
    `,
  ],
})
export class ImageBlockEditorComponent {
  @Input({ required: true }) data!: ImageBlockData;
  @Input() blockId = '';
  @Output() dataChange = new EventEmitter<ImageBlockData>();

  update(patch: Partial<ImageBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }

  onUploaded(att: AttachmentDto) {
    this.update({ url: att.fileUrl });
  }
}

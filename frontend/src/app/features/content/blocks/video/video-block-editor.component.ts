import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VideoBlockData } from '../../models';
import { FileUploaderComponent } from '../../../../shared/components/file-uploader/file-uploader.component';
import { AttachmentDto } from '../../../../core/models/attachment.model';

type Mode = 'url' | 'upload';

@Component({
  selector: 'app-video-block-editor',
  standalone: true,
  imports: [FormsModule, FileUploaderComponent],
  template: `
    <div class="mode-tabs">
      <button type="button" [class.active]="mode() === 'url'" (click)="mode.set('url')">URL</button>
      <button type="button" [class.active]="mode() === 'upload'" (click)="mode.set('upload')">Загрузить</button>
    </div>

    @if (mode() === 'url') {
      <input
        type="url"
        class="input"
        placeholder="https://..."
        [ngModel]="data.url"
        (ngModelChange)="update({ url: $event })"
      />
    } @else {
      <app-file-uploader
        accept="video/*"
        entityType="LessonBlock"
        [entityId]="blockId"
        (fileUploaded)="onUploaded($event)"
      ></app-file-uploader>
    }

    <input
      type="text"
      class="input"
      placeholder="Подпись (опционально)"
      [ngModel]="data.caption"
      (ngModelChange)="update({ caption: $event })"
    />

    @if (data.url) {
      <video controls [src]="data.url" class="preview"></video>
    }
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .mode-tabs { display: flex; gap: 4px; }
      .mode-tabs button {
        padding: 6px 12px; border: 1px solid #E2E8F0; background: #fff;
        border-radius: 8px; cursor: pointer; font-size: 0.875rem; color: #64748B;
      }
      .mode-tabs button.active { background: #4F46E5; border-color: #4F46E5; color: #fff; }
      .input {
        padding: 8px 12px; border: 1px solid #E2E8F0; border-radius: 8px;
        background: #F8FAFC; font: inherit; font-size: 0.875rem; outline: none;
      }
      .input:focus { border-color: #4F46E5; background: #fff; }
      .preview { width: 100%; max-height: 240px; border-radius: 8px; background: #000; }
    `,
  ],
})
export class VideoBlockEditorComponent {
  @Input({ required: true }) data!: VideoBlockData;
  @Input() blockId = '';
  @Output() dataChange = new EventEmitter<VideoBlockData>();

  mode = signal<Mode>('url');

  update(patch: Partial<VideoBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }

  onUploaded(att: AttachmentDto) {
    this.update({ url: att.fileUrl });
  }
}

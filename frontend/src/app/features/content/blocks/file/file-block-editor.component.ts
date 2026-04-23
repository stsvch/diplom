import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FileBlockData } from '../../models';
import { FileUploaderComponent } from '../../../../shared/components/file-uploader/file-uploader.component';
import { AttachmentDto } from '../../../../core/models/attachment.model';

@Component({
  selector: 'app-file-block-editor',
  standalone: true,
  imports: [FormsModule, FileUploaderComponent],
  template: `
    @if (!data.attachmentId) {
      <app-file-uploader
        accept="*/*"
        entityType="LessonBlock"
        [entityId]="blockId"
        (fileUploaded)="onUploaded($event)"
      ></app-file-uploader>
    } @else {
      <div class="attached">
        <span>Файл прикреплён</span>
        <button type="button" (click)="update({ attachmentId: '' })">Удалить</button>
      </div>
    }

    <input
      type="text"
      class="input"
      placeholder="Отображаемое имя (опц.)"
      [ngModel]="data.displayName"
      (ngModelChange)="update({ displayName: $event })"
    />
    <textarea
      class="input textarea"
      rows="2"
      placeholder="Описание (опц.)"
      [ngModel]="data.description"
      (ngModelChange)="update({ description: $event })"
    ></textarea>
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .input {
        padding: 8px 12px; border: 1px solid #E2E8F0; border-radius: 8px;
        background: #F8FAFC; font: inherit; font-size: 0.875rem; outline: none;
      }
      .input:focus { border-color: #4F46E5; background: #fff; }
      .textarea { resize: vertical; }
      .attached {
        display: flex; align-items: center; justify-content: space-between;
        padding: 12px; background: #DCFCE7; color: #166534; border-radius: 8px;
      }
      .attached button {
        padding: 4px 10px; background: #fff; border: 1px solid #E2E8F0;
        border-radius: 6px; cursor: pointer; font-size: 0.75rem; color: #EF4444;
      }
    `,
  ],
})
export class FileBlockEditorComponent {
  @Input({ required: true }) data!: FileBlockData;
  @Input() blockId = '';
  @Output() dataChange = new EventEmitter<FileBlockData>();

  update(patch: Partial<FileBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }

  onUploaded(att: AttachmentDto) {
    this.update({ attachmentId: att.id, displayName: att.fileName });
  }
}

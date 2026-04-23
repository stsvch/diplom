import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { LucideAngularModule } from 'lucide-angular';
import { FileService } from '../../../../core/services/file.service';
import { AttachmentDto } from '../../../../core/models/attachment.model';
import { FileBlockData } from '../../models';

@Component({
  selector: 'app-file-block-viewer',
  standalone: true,
  imports: [LucideAngularModule],
  template: `
    @if (attachment(); as att) {
      <a class="file-card" [href]="att.fileUrl" target="_blank" rel="noopener">
        <lucide-icon name="file" size="24"></lucide-icon>
        <div class="info">
          <div class="name">{{ data.displayName || att.fileName }}</div>
          <div class="meta">{{ fileSize(att.fileSize) }}</div>
          @if (data.description) {
            <div class="desc">{{ data.description }}</div>
          }
        </div>
        <lucide-icon name="download" size="18"></lucide-icon>
      </a>
    } @else if (!data.attachmentId) {
      <div class="placeholder">Файл не прикреплён</div>
    } @else {
      <div class="placeholder">Загрузка информации о файле...</div>
    }
  `,
  styles: [
    `
      :host { display: block; }
      .file-card {
        display: flex; align-items: center; gap: 16px; padding: 16px;
        border: 1px solid #E2E8F0; border-radius: 12px; background: #F8FAFC;
        color: #0F172A; text-decoration: none; transition: all 0.15s ease;
      }
      .file-card:hover { border-color: #4F46E5; background: #EEF2FF; }
      .info { flex: 1; }
      .name { font-weight: 600; font-size: 0.9375rem; }
      .meta { font-size: 0.75rem; color: #64748B; margin-top: 2px; }
      .desc { font-size: 0.875rem; color: #475569; margin-top: 4px; }
      .placeholder { padding: 16px; background: #F1F5F9; border-radius: 8px; color: #64748B; font-size: 0.875rem; }
    `,
  ],
})
export class FileBlockViewerComponent implements OnInit {
  @Input({ required: true }) data!: FileBlockData;

  private readonly fileService = inject(FileService);
  attachment = signal<AttachmentDto | null>(null);

  ngOnInit() {
    if (!this.data.attachmentId) return;
    this.fileService.getFileInfo(this.data.attachmentId).subscribe({
      next: (att) => this.attachment.set(att),
    });
  }

  fileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} Б`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} КБ`;
    return `${(bytes / 1024 / 1024).toFixed(1)} МБ`;
  }
}

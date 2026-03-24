import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  LucideAngularModule,
  Download,
  Trash2,
  FileText,
  Image,
  Film,
  Music,
  Archive,
  FileCode,
  File,
} from 'lucide-angular';
import { AttachmentDto } from '../../../core/models/attachment.model';
import { FileService } from '../../../core/services/file.service';

@Component({
  selector: 'app-file-card',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './file-card.component.html',
  styleUrl: './file-card.component.scss',
})
export class FileCardComponent {
  @Input({ required: true }) attachment!: AttachmentDto;
  @Input() deletable = false;
  @Output() deleted = new EventEmitter<string>();

  private readonly fileService = inject(FileService);

  readonly DownloadIcon = Download;
  readonly TrashIcon = Trash2;
  readonly FileTextIcon = FileText;
  readonly ImageIcon = Image;
  readonly FilmIcon = Film;
  readonly MusicIcon = Music;
  readonly ArchiveIcon = Archive;
  readonly FileCodeIcon = FileCode;
  readonly FileIcon = File;

  get fileIcon() {
    const type = this.attachment.contentType ?? '';
    if (type.startsWith('image/')) return this.ImageIcon;
    if (type.startsWith('video/')) return this.FilmIcon;
    if (type.startsWith('audio/')) return this.MusicIcon;
    if (
      type === 'application/pdf' ||
      type.includes('word') ||
      type.includes('document') ||
      type.includes('text')
    )
      return this.FileTextIcon;
    if (type.includes('zip') || type.includes('archive') || type.includes('tar'))
      return this.ArchiveIcon;
    if (type.includes('json') || type.includes('javascript') || type.includes('html'))
      return this.FileCodeIcon;
    return this.FileIcon;
  }

  get iconColorClass(): string {
    const type = this.attachment.contentType ?? '';
    if (type.startsWith('image/')) return 'icon--image';
    if (type.startsWith('video/')) return 'icon--video';
    if (type.startsWith('audio/')) return 'icon--audio';
    if (type === 'application/pdf') return 'icon--pdf';
    if (type.includes('word') || type.includes('document')) return 'icon--doc';
    if (type.includes('zip') || type.includes('archive')) return 'icon--archive';
    return 'icon--default';
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
    return `${(bytes / (1024 * 1024 * 1024)).toFixed(1)} GB`;
  }

  get downloadUrl(): string {
    return this.fileService.getDownloadUrl(this.attachment.id);
  }

  onDelete(): void {
    this.deleted.emit(this.attachment.id);
  }
}

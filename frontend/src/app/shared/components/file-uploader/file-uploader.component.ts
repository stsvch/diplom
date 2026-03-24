import {
  Component,
  Input,
  Output,
  EventEmitter,
  inject,
  signal,
  ElementRef,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  LucideAngularModule,
  Upload,
  X,
  FileText,
  Image,
  Film,
  File,
  Loader2,
  AlertCircle,
} from 'lucide-angular';
import { FileService } from '../../../core/services/file.service';
import { AttachmentDto } from '../../../core/models/attachment.model';

@Component({
  selector: 'app-file-uploader',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './file-uploader.component.html',
  styleUrl: './file-uploader.component.scss',
})
export class FileUploaderComponent {
  @Input() accept = '*/*';
  @Input() maxSize = 100 * 1024 * 1024; // 100MB
  @Input() multiple = false;
  @Input() entityType = '';
  @Input() entityId = '';

  @Output() fileUploaded = new EventEmitter<AttachmentDto>();

  @ViewChild('fileInput') fileInputRef!: ElementRef<HTMLInputElement>;

  private readonly fileService = inject(FileService);

  readonly UploadIcon = Upload;
  readonly XIcon = X;
  readonly FileTextIcon = FileText;
  readonly ImageIcon = Image;
  readonly FilmIcon = Film;
  readonly FileIcon = File;
  readonly Loader2Icon = Loader2;
  readonly AlertCircleIcon = AlertCircle;

  readonly isDragOver = signal(false);
  readonly uploading = signal(false);
  readonly errorMessage = signal('');
  readonly previewUrl = signal<string | null>(null);
  readonly previewIsImage = signal(false);
  readonly uploadedFile = signal<AttachmentDto | null>(null);

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver.set(false);
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      if (this.multiple) {
        Array.from(files).forEach((f) => this.processFile(f));
      } else {
        this.processFile(files[0]);
      }
    }
  }

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      if (this.multiple) {
        Array.from(input.files).forEach((f) => this.processFile(f));
      } else {
        this.processFile(input.files[0]);
      }
    }
    input.value = '';
  }

  openFilePicker(): void {
    this.fileInputRef.nativeElement.click();
  }

  private processFile(file: File): void {
    this.errorMessage.set('');

    if (!this.isAccepted(file)) {
      this.errorMessage.set(`Неподдерживаемый тип файла: ${file.type || file.name}`);
      return;
    }

    if (file.size > this.maxSize) {
      const maxMb = (this.maxSize / (1024 * 1024)).toFixed(0);
      this.errorMessage.set(`Файл слишком большой. Максимум: ${maxMb} MB`);
      return;
    }

    if (file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onload = (e) => {
        this.previewUrl.set(e.target?.result as string);
        this.previewIsImage.set(true);
      };
      reader.readAsDataURL(file);
    } else {
      this.previewUrl.set(null);
      this.previewIsImage.set(false);
    }

    this.uploading.set(true);
    this.fileService.upload(file, this.entityType, this.entityId).subscribe({
      next: (attachment) => {
        this.uploading.set(false);
        this.uploadedFile.set(attachment);
        this.fileUploaded.emit(attachment);
      },
      error: (err) => {
        this.uploading.set(false);
        this.previewUrl.set(null);
        this.previewIsImage.set(false);
        this.errorMessage.set(err?.message ?? 'Ошибка загрузки файла');
      },
    });
  }

  private isAccepted(file: File): boolean {
    if (!this.accept || this.accept === '*/*' || this.accept === '*') return true;
    const parts = this.accept.split(',').map((p) => p.trim());
    return parts.some((pattern) => {
      if (pattern.endsWith('/*')) {
        const baseType = pattern.slice(0, -2);
        return file.type.startsWith(baseType + '/');
      }
      if (pattern.startsWith('.')) {
        return file.name.toLowerCase().endsWith(pattern.toLowerCase());
      }
      return file.type === pattern;
    });
  }

  clearUpload(): void {
    this.uploadedFile.set(null);
    this.previewUrl.set(null);
    this.previewIsImage.set(false);
    this.errorMessage.set('');
  }
}

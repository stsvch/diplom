import {
  Component,
  inject,
  signal,
  OnInit,
  OnDestroy,
} from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import {
  LucideAngularModule,
  ChevronLeft,
  Plus,
  Trash2,
  ArrowUp,
  ArrowDown,
  Type,
  Video,
  Paperclip,
  Save,
  Loader2,
  ChevronDown,
} from 'lucide-angular';
import { CoursesService } from '../services/courses.service';
import { FileService } from '../../../core/services/file.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { LessonBlockDto } from '../models/course.model';
import { AttachmentDto } from '../../../core/models/attachment.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { RichTextEditorComponent } from '../../../shared/components/rich-text-editor/rich-text-editor.component';
import { VideoPlayerComponent } from '../../../shared/components/video-player/video-player.component';
import { FileUploaderComponent } from '../../../shared/components/file-uploader/file-uploader.component';
import { FileCardComponent } from '../../../shared/components/file-card/file-card.component';

interface BlockWithFiles extends LessonBlockDto {
  files?: AttachmentDto[];
  filesLoading?: boolean;
  videoInputMode?: 'url' | 'upload';
  pendingVideoUrl?: string;
}

@Component({
  selector: 'app-lesson-editor',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    LucideAngularModule,
    ButtonComponent,
    BadgeComponent,
    RichTextEditorComponent,
    VideoPlayerComponent,
    FileUploaderComponent,
    FileCardComponent,
  ],
  templateUrl: './lesson-editor.component.html',
  styleUrl: './lesson-editor.component.scss',
})
export class LessonEditorComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly coursesService = inject(CoursesService);
  private readonly fileService = inject(FileService);
  private readonly toastService = inject(ToastService);

  readonly ChevronLeftIcon = ChevronLeft;
  readonly PlusIcon = Plus;
  readonly TrashIcon = Trash2;
  readonly ArrowUpIcon = ArrowUp;
  readonly ArrowDownIcon = ArrowDown;
  readonly TypeIcon = Type;
  readonly VideoIcon = Video;
  readonly PaperclipIcon = Paperclip;
  readonly SaveIcon = Save;
  readonly Loader2Icon = Loader2;
  readonly ChevronDownIcon = ChevronDown;

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly showAddMenu = signal(false);

  readonly blocks = signal<BlockWithFiles[]>([]);
  readonly lessonTitle = signal('Урок');

  lessonId = '';
  courseId = '';

  private readonly autoSave$ = new Subject<BlockWithFiles>();
  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.lessonId = this.route.snapshot.paramMap.get('id') ?? '';
    this.courseId = this.route.snapshot.queryParamMap.get('courseId') ?? '';
    if (this.lessonId) {
      this.loadBlocks();
    }

    this.autoSave$.pipe(debounceTime(1500), takeUntil(this.destroy$)).subscribe((block) => {
      this.saveBlock(block);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadBlocks(): void {
    this.loading.set(true);
    this.coursesService.getLessonBlocks(this.lessonId).subscribe({
      next: (data) => {
        const sorted = data.sort((a, b) => a.orderIndex - b.orderIndex);
        const withExtras: BlockWithFiles[] = sorted.map((b) => ({
          ...b,
          files: [],
          filesLoading: false,
          videoInputMode: 'url',
          pendingVideoUrl: b.videoUrl ?? '',
        }));
        this.blocks.set(withExtras);
        this.loading.set(false);

        // Load files for File blocks
        withExtras.forEach((block) => {
          if (block.type === 'File') {
            this.loadBlockFiles(block);
          }
        });
      },
      error: (err: ApiError) => {
        this.loading.set(false);
        this.toastService.error(err.message);
      },
    });
  }

  loadBlockFiles(block: BlockWithFiles): void {
    this.updateBlock(block.id, { filesLoading: true });
    this.fileService.getEntityFiles('LessonBlock', block.id).subscribe({
      next: (files) => {
        this.updateBlock(block.id, { files, filesLoading: false });
      },
      error: () => {
        this.updateBlock(block.id, { filesLoading: false });
      },
    });
  }

  addBlock(type: 'Text' | 'Video' | 'File'): void {
    this.showAddMenu.set(false);
    const currentBlocks = this.blocks();
    const orderIndex = currentBlocks.length;

    this.coursesService
      .createLessonBlock(this.lessonId, {
        type,
        orderIndex,
        textContent: type === 'Text' ? '' : undefined,
        videoUrl: type === 'Video' ? '' : undefined,
      })
      .subscribe({
        next: (block) => {
          const newBlock: BlockWithFiles = {
            ...block,
            files: [],
            filesLoading: false,
            videoInputMode: 'url',
            pendingVideoUrl: block.videoUrl ?? '',
          };
          this.blocks.update((bs) => [...bs, newBlock]);
          this.toastService.success('Блок добавлен');
        },
        error: (err: ApiError) => {
          this.toastService.error(err.message);
        },
      });
  }

  deleteBlock(blockId: string): void {
    this.coursesService.deleteLessonBlock(blockId).subscribe({
      next: () => {
        this.blocks.update((bs) => bs.filter((b) => b.id !== blockId));
        this.toastService.success('Блок удалён');
      },
      error: (err: ApiError) => {
        this.toastService.error(err.message);
      },
    });
  }

  moveBlock(index: number, direction: 'up' | 'down'): void {
    const blocks = [...this.blocks()];
    const target = direction === 'up' ? index - 1 : index + 1;
    if (target < 0 || target >= blocks.length) return;
    [blocks[index], blocks[target]] = [blocks[target], blocks[index]];
    blocks.forEach((b, i) => (b.orderIndex = i));
    this.blocks.set(blocks);
    this.saveReorder();
  }

  onTextChange(blockId: string, content: string): void {
    this.updateBlock(blockId, { textContent: content });
    const block = this.blocks().find((b) => b.id === blockId);
    if (block) this.autoSave$.next({ ...block, textContent: content });
  }

  onVideoUrlChange(blockId: string, url: string): void {
    this.updateBlock(blockId, { pendingVideoUrl: url });
  }

  applyVideoUrl(block: BlockWithFiles): void {
    const url = block.pendingVideoUrl ?? '';
    this.updateBlock(block.id, { videoUrl: url });
    this.saveBlock({ ...block, videoUrl: url });
  }

  setVideoMode(blockId: string, mode: 'url' | 'upload'): void {
    this.updateBlock(blockId, { videoInputMode: mode });
  }

  onVideoFileUploaded(blockId: string, attachment: AttachmentDto): void {
    const url = this.fileService.getDownloadUrl(attachment.id);
    this.updateBlock(blockId, { videoUrl: url, pendingVideoUrl: url });
    const block = this.blocks().find((b) => b.id === blockId);
    if (block) this.saveBlock({ ...block, videoUrl: url });
  }

  onFileUploaded(blockId: string, attachment: AttachmentDto): void {
    this.updateBlock(blockId, {
      files: [...(this.blocks().find((b) => b.id === blockId)?.files ?? []), attachment],
    });
  }

  deleteAttachment(blockId: string, fileId: string): void {
    this.fileService.deleteFile(fileId).subscribe({
      next: () => {
        this.updateBlock(blockId, {
          files: this.blocks()
            .find((b) => b.id === blockId)
            ?.files?.filter((f) => f.id !== fileId) ?? [],
        });
        this.toastService.success('Файл удалён');
      },
      error: (err: ApiError) => {
        this.toastService.error(err.message);
      },
    });
  }

  saveBlock(block: BlockWithFiles): void {
    this.coursesService
      .updateLessonBlock(block.id, {
        type: block.type,
        orderIndex: block.orderIndex,
        textContent: block.textContent,
        videoUrl: block.videoUrl,
      })
      .subscribe({
        error: (err: ApiError) => {
          this.toastService.error(err.message);
        },
      });
  }

  saveAll(): void {
    this.saving.set(true);
    const blocks = this.blocks();
    let pending = blocks.length;
    if (pending === 0) {
      this.saving.set(false);
      this.toastService.success('Сохранено');
      return;
    }
    blocks.forEach((block) => {
      this.coursesService
        .updateLessonBlock(block.id, {
          type: block.type,
          orderIndex: block.orderIndex,
          textContent: block.textContent,
          videoUrl: block.videoUrl,
        })
        .subscribe({
          next: () => {
            pending--;
            if (pending === 0) {
              this.saving.set(false);
              this.toastService.success('Все блоки сохранены');
            }
          },
          error: (err: ApiError) => {
            pending--;
            if (pending === 0) this.saving.set(false);
            this.toastService.error(err.message);
          },
        });
    });
  }

  private saveReorder(): void {
    const ids = this.blocks().map((b) => b.id);
    this.coursesService
      .reorderLessonBlocks(this.lessonId, ids)
      .subscribe({
        error: (err: ApiError) => {
          this.toastService.error(err.message);
        },
      });
  }

  private updateBlock(id: string, changes: Partial<BlockWithFiles>): void {
    this.blocks.update((bs) =>
      bs.map((b) => (b.id === id ? { ...b, ...changes } : b)),
    );
  }

  getBlockTypeLabel(type: string): string {
    switch (type) {
      case 'Text': return 'Текст';
      case 'Video': return 'Видео';
      case 'File': return 'Файл';
      default: return type;
    }
  }

  getBlockTypeBadgeVariant(type: string): 'primary' | 'success' | 'warning' {
    switch (type) {
      case 'Text': return 'primary';
      case 'Video': return 'success';
      case 'File': return 'warning';
      default: return 'primary';
    }
  }

  toggleAddMenu(): void {
    this.showAddMenu.update((v) => !v);
  }

  closeAddMenu(): void {
    this.showAddMenu.set(false);
  }

  get backUrl(): string {
    return this.courseId ? `/teacher/courses/edit/${this.courseId}` : '/teacher/courses';
  }
}

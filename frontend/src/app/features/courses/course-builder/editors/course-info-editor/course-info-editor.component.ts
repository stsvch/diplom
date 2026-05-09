import { Component, Input, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Info, Upload, X, LucideAngularModule } from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';
import { FileService } from '../../../../../core/services/file.service';
import { ToastService } from '../../../../../shared/components/toast/toast.service';
import { TagInputComponent } from '../../../../../shared/components/tag-input/tag-input.component';

@Component({
  selector: 'app-cb-course-info-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, TagInputComponent],
  templateUrl: './course-info-editor.component.html',
  styleUrl: './course-info-editor.component.scss',
})
export class CourseInfoEditorComponent {
  @Input({ required: true }) store!: CourseBuilderStore;

  private readonly files = inject(FileService);
  private readonly toast = inject(ToastService);

  readonly icons = { info: Info, upload: Upload, close: X };
  readonly uploading = signal(false);

  setTitle(value: string): void {
    this.store.patchCourseInfo({ title: value });
  }

  setDescription(value: string): void {
    this.store.patchCourseInfo({ description: value });
  }

  setImageUrl(value: string): void {
    this.store.patchCourseInfo({ imageUrl: value });
  }

  clearImage(): void {
    this.store.patchCourseInfo({ imageUrl: '' });
  }

  setTags(value: string[]): void {
    this.store.patchCourseInfo({ tags: value });
  }

  setIsFree(value: boolean): void {
    this.store.patchCourseInfo({ isFree: value, price: value ? null : 0 });
  }

  setPrice(value: string): void {
    const n = Number(value);
    if (!Number.isNaN(n)) {
      this.store.patchCourseInfo({ price: n, isFree: false });
    }
  }

  setHasGrading(value: boolean): void {
    this.store.patchCourseInfo({ hasGrading: value });
  }

  // ── Cover upload ───────────────────────────────────────

  onPickCover(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (file) this.uploadCover(file);
  }

  onDropCover(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const file = event.dataTransfer?.files?.[0];
    if (file) this.uploadCover(file);
  }

  onDragOverCover(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  private uploadCover(file: File): void {
    const validation = this.validateImage(file);
    if (validation) {
      this.toast.error(validation);
      return;
    }
    const courseId = this.store.courseId();
    if (!courseId) {
      this.toast.error('Курс ещё не сохранён');
      return;
    }

    this.uploading.set(true);
    this.files.upload(file, 'CourseCover', courseId).subscribe({
      next: (att) => {
        this.uploading.set(false);
        this.store.patchCourseInfo({ imageUrl: att.fileUrl });
      },
      error: (err) => {
        this.uploading.set(false);
        const msg = err?.error?.message ?? err?.message ?? 'Не удалось загрузить обложку';
        this.toast.error(msg);
      },
    });
  }

  private validateImage(file: File): string | null {
    if (!file.type.startsWith('image/')) {
      return 'Нужен файл изображения (PNG, JPG, WebP).';
    }
    if (file.size > 5 * 1024 * 1024) {
      return 'Максимальный размер обложки — 5 МБ.';
    }
    return null;
  }
}

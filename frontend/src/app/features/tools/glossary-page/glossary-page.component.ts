import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { parseApiError } from '../../../core/models/api-error.model';
import { UserRole } from '../../../core/models/user.model';
import { AuthService } from '../../../core/services/auth.service';
import { TagInputComponent } from '../../../shared/components/tag-input/tag-input.component';
import { CourseListDto } from '../../courses/models/course.model';
import { CoursesService } from '../../courses/services/courses.service';
import {
  DictionaryReviewOutcome,
  DictionaryWordDto,
  UpsertDictionaryWordRequest,
} from '../models/glossary.model';
import { GlossaryService } from '../services/glossary.service';

interface GlossaryEditorModel {
  courseId: string;
  term: string;
  translation: string;
  definition: string;
  example: string;
  note: string;
  tags: string[];
  imageUrl: string | null;
}

@Component({
  selector: 'app-glossary-page',
  standalone: true,
  imports: [CommonModule, FormsModule, TagInputComponent],
  templateUrl: './glossary-page.component.html',
  styleUrl: './glossary-page.component.scss',
})
export class GlossaryPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly coursesService = inject(CoursesService);
  private readonly glossaryService = inject(GlossaryService);
  private readonly studyBatchSize = 12;

  readonly userRole = this.authService.userRole;
  readonly isTeacher = computed(() => this.userRole() === UserRole.Teacher);
  readonly isStudent = computed(() => this.userRole() === UserRole.Student);
  readonly loading = signal(true);
  readonly reloading = signal(false);
  readonly saving = signal(false);
  readonly deletingWordId = signal<string | null>(null);
  readonly progressWordId = signal<string | null>(null);
  readonly editingWordId = signal<string | null>(null);
  readonly creatorOpen = signal(false);
  readonly imageUploading = signal(false);
  readonly imageDeleting = signal(false);
  readonly dragActive = signal(false);
  readonly error = signal<string | null>(null);
  readonly courses = signal<CourseListDto[]>([]);
  readonly words = signal<DictionaryWordDto[]>([]);
  readonly selectedCourseId = signal('');
  readonly search = signal('');
  readonly knownOnly = signal(false);
  readonly studyMode = signal(false);
  readonly studyLoading = signal(false);
  readonly studySaving = signal(false);
  readonly studyIndex = signal(0);
  readonly showStudyAnswer = signal(false);
  readonly studyQueue = signal<DictionaryWordDto[]>([]);
  readonly studySeenWordIds = signal<string[]>([]);
  readonly studyCompletedCount = signal(0);

  editor: GlossaryEditorModel = this.createEmptyEditor();
  private pendingImageFile: File | null = null;

  readonly currentStudyWord = computed(() => {
    const queue = this.studyQueue();
    const index = this.studyIndex();
    return index >= 0 && index < queue.length ? queue[index] : null;
  });

  readonly studyStats = computed(() => ({
    total: this.studyCompletedCount() + this.studyQueue().length,
    current: this.currentStudyWord() ? this.studyCompletedCount() + 1 : this.studyCompletedCount(),
    remaining: Math.max(this.studyQueue().length - 1, 0),
  }));

  get previewWord(): DictionaryWordDto {
    return {
      id: 'preview',
      courseId: this.editor.courseId,
      courseTitle: this.courses().find((c) => c.id === this.editor.courseId)?.title ?? '',
      term: this.editor.term.trim() || 'Заголовок карточки',
      translation: this.editor.translation.trim() || null,
      definition: this.editor.definition.trim() || null,
      example: this.editor.example.trim() || null,
      note: this.editor.note.trim() || null,
      imageUrl: this.editor.imageUrl,
      tags: this.editor.tags,
      createdById: '',
      isKnown: false,
      reviewCount: 0,
      hardCount: 0,
      repeatLaterCount: 0,
      lastReviewedAt: null,
      lastOutcome: null,
      nextReviewAt: null,
      createdAt: new Date().toISOString(),
      updatedAt: null,
    };
  }

  readonly wordsCountLabel = computed(() => {
    const n = this.words().length;
    const mod10 = n % 10;
    const mod100 = n % 100;
    if (mod10 === 1 && mod100 !== 11) return `${n} карточка`;
    if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return `${n} карточки`;
    return `${n} карточек`;
  });

  ngOnInit(): void {
    this.loadCourses();
  }

  reloadWords(): void {
    this.error.set(null);
    this.reloading.set(true);

    this.glossaryService.getWords({
      courseId: this.selectedCourseId() || undefined,
      search: this.search().trim() || undefined,
      knownOnly: this.isStudent() ? this.knownOnly() : undefined,
    }).subscribe({
      next: (words) => {
        this.words.set(words);
        this.loading.set(false);
        this.reloading.set(false);
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loading.set(false);
        this.reloading.set(false);
      },
    });
  }

  startStudyMode(): void {
    this.error.set(null);
    this.studyMode.set(true);
    this.showStudyAnswer.set(false);
    this.studyIndex.set(0);
    this.studyQueue.set([]);
    this.studySeenWordIds.set([]);
    this.studyCompletedCount.set(0);
    this.loadStudyBatch(true);
  }

  exitStudyMode(): void {
    this.studyMode.set(false);
    this.studyLoading.set(false);
    this.studySaving.set(false);
    this.studyQueue.set([]);
    this.studyIndex.set(0);
    this.showStudyAnswer.set(false);
    this.studySeenWordIds.set([]);
    this.studyCompletedCount.set(0);
  }

  revealStudyAnswer(): void {
    this.showStudyAnswer.set(true);
  }

  startCreate(): void {
    this.editingWordId.set(null);
    this.editor = this.createEmptyEditor();
    this.pendingImageFile = null;
    if (!this.editor.courseId) {
      this.editor.courseId = this.selectedCourseId() || this.courses()[0]?.id || '';
    }
    this.error.set(null);
    this.creatorOpen.set(true);
  }

  editWord(word: DictionaryWordDto): void {
    this.editingWordId.set(word.id);
    this.editor = {
      courseId: word.courseId,
      term: word.term,
      translation: word.translation ?? '',
      definition: word.definition ?? '',
      example: word.example ?? '',
      note: word.note ?? '',
      tags: [...word.tags],
      imageUrl: word.imageUrl ?? null,
    };
    this.pendingImageFile = null;
    this.error.set(null);
    this.creatorOpen.set(true);
  }

  closeCreator(): void {
    this.creatorOpen.set(false);
    this.editingWordId.set(null);
    this.editor = this.createEmptyEditor();
    this.pendingImageFile = null;
    this.error.set(null);
  }

  saveWord(): void {
    if (!this.editor.term.trim()) {
      this.error.set('Заголовок обязателен.');
      return;
    }
    if (!this.editor.courseId) {
      this.error.set('Выберите курс.');
      return;
    }

    this.error.set(null);
    this.saving.set(true);

    const payload: UpsertDictionaryWordRequest = {
      courseId: this.editor.courseId,
      term: this.editor.term.trim(),
      translation: this.editor.translation.trim() || null,
      definition: this.editor.definition.trim() || null,
      example: this.editor.example.trim() || null,
      note: this.editor.note.trim() || null,
      tags: this.editor.tags,
    };

    const editingId = this.editingWordId();
    const request$ = editingId
      ? this.glossaryService.updateWord(editingId, payload)
      : this.glossaryService.createWord(payload);

    request$.subscribe({
      next: (saved) => {
        if (!editingId && this.pendingImageFile) {
          this.uploadPendingImage(saved);
        } else {
          this.saving.set(false);
          this.closeCreator();
          this.reloadWords();
        }
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.saving.set(false);
      },
    });
  }

  private uploadPendingImage(saved: DictionaryWordDto): void {
    const file = this.pendingImageFile!;
    this.glossaryService.uploadImage(saved.id, file).subscribe({
      next: () => {
        this.pendingImageFile = null;
        this.saving.set(false);
        this.closeCreator();
        this.reloadWords();
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.saving.set(false);
        this.pendingImageFile = null;
        this.editingWordId.set(saved.id);
        this.editor.imageUrl = null;
        this.reloadWords();
      },
    });
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (file) {
      this.processSelectedImage(file);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragActive.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragActive.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragActive.set(false);
    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.processSelectedImage(file);
    }
  }

  private processSelectedImage(file: File): void {
    if (!file.type.startsWith('image/')) {
      this.error.set('Поддерживаются только изображения.');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.error.set('Размер картинки не должен превышать 5 МБ.');
      return;
    }

    const wordId = this.editingWordId();
    if (wordId) {
      this.error.set(null);
      this.imageUploading.set(true);
      this.glossaryService.uploadImage(wordId, file).subscribe({
        next: (updated) => {
          this.imageUploading.set(false);
          this.editor.imageUrl = updated.imageUrl ?? null;
          this.updateWordInList(updated);
        },
        error: (err) => {
          this.error.set(parseApiError(err).message);
          this.imageUploading.set(false);
        },
      });
      return;
    }

    this.error.set(null);
    this.pendingImageFile = file;
    const reader = new FileReader();
    reader.onload = () => {
      this.editor.imageUrl = reader.result as string;
    };
    reader.readAsDataURL(file);
  }

  removeImage(): void {
    const wordId = this.editingWordId();
    if (!wordId) {
      this.editor.imageUrl = null;
      this.pendingImageFile = null;
      return;
    }

    this.error.set(null);
    this.imageDeleting.set(true);

    this.glossaryService.deleteImage(wordId).subscribe({
      next: (updated) => {
        this.imageDeleting.set(false);
        this.editor.imageUrl = null;
        this.updateWordInList(updated);
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.imageDeleting.set(false);
      },
    });
  }

  onTagsChanged(tags: string[]): void {
    this.editor.tags = tags;
  }

  deleteWord(wordId: string): void {
    if (!confirm('Удалить эту карточку?')) return;
    this.error.set(null);
    this.deletingWordId.set(wordId);

    this.glossaryService.deleteWord(wordId).subscribe({
      next: () => {
        this.deletingWordId.set(null);
        if (this.editingWordId() === wordId) {
          this.closeCreator();
        }
        this.reloadWords();
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.deletingWordId.set(null);
      },
    });
  }

  toggleKnown(word: DictionaryWordDto): void {
    this.error.set(null);
    this.progressWordId.set(word.id);

    this.glossaryService.setProgress(word.id, !word.isKnown).subscribe({
      next: (updated) => {
        this.progressWordId.set(null);
        this.words.update((current) => {
          const next = current.map((item) => (item.id === updated.id ? updated : item));
          return this.knownOnly() ? next.filter((item) => item.isKnown) : next;
        });
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.progressWordId.set(null);
      },
    });
  }

  reviewCurrentWord(outcome: DictionaryReviewOutcome): void {
    const current = this.currentStudyWord();
    if (!current) return;

    this.error.set(null);
    this.studySaving.set(true);

    this.glossaryService.reviewWord(current.id, outcome).subscribe({
      next: (updated) => {
        this.studySaving.set(false);
        this.showStudyAnswer.set(false);
        this.updateWordInList(updated);
        this.advanceStudyQueue(updated);
        this.maybeRefillStudyQueue();
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.studySaving.set(false);
      },
    });
  }

  trackWord(_: number, word: DictionaryWordDto): string {
    return word.id;
  }

  trackTag(_: number, tag: string): string {
    return tag;
  }

  private loadCourses(): void {
    this.loading.set(true);
    this.error.set(null);

    this.coursesService.getMyCourses().subscribe({
      next: (courses) => {
        this.courses.set(courses);

        if (courses.length === 1) {
          this.selectedCourseId.set(courses[0].id);
        }

        if (!this.editor.courseId) {
          this.editor.courseId = this.selectedCourseId() || courses[0]?.id || '';
        }

        this.reloadWords();
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loading.set(false);
      },
    });
  }

  private createEmptyEditor(): GlossaryEditorModel {
    return {
      courseId: this.selectedCourseId() || this.courses()[0]?.id || '',
      term: '',
      translation: '',
      definition: '',
      example: '',
      note: '',
      tags: [],
      imageUrl: null,
    };
  }

  private advanceStudyQueue(updated: DictionaryWordDto): void {
    const currentIndex = this.studyIndex();
    const queue = [...this.studyQueue()];
    if (currentIndex < 0 || currentIndex >= queue.length) return;

    const currentWordId = updated.id;
    queue.splice(currentIndex, 1);

    this.studyQueue.set(queue);
    this.studySeenWordIds.update((ids) =>
      ids.includes(currentWordId) ? ids : [...ids, currentWordId],
    );
    this.studyCompletedCount.update((count) => count + 1);

    if (queue.length === 0) {
      this.studyIndex.set(0);
      return;
    }

    this.studyIndex.set(Math.min(currentIndex, queue.length - 1));
  }

  private updateWordInList(updated: DictionaryWordDto): void {
    this.words.update((current) => {
      const next = current.map((item) => (item.id === updated.id ? updated : item));
      return this.knownOnly() ? next.filter((item) => item.isKnown) : next;
    });
  }

  private maybeRefillStudyQueue(force = false): void {
    if (!this.studyMode()) return;
    if (!force && this.studyQueue().length >= 4) return;
    if (this.studyLoading()) return;
    this.loadStudyBatch(false);
  }

  private loadStudyBatch(reset: boolean): void {
    this.studyLoading.set(true);

    const excludeWordIds = Array.from(
      new Set([
        ...this.studySeenWordIds(),
        ...this.studyQueue().map((word) => word.id),
      ]),
    );

    this.glossaryService
      .getReviewSession(this.selectedCourseId() || undefined, this.studyBatchSize, excludeWordIds)
      .subscribe({
        next: (batch) => {
          if (reset) {
            this.studyQueue.set(batch);
            this.studyIndex.set(0);
          } else if (batch.length > 0) {
            this.studyQueue.update((current) => [...current, ...batch]);
            if (this.studyQueue().length === batch.length) {
              this.studyIndex.set(0);
            }
          }
          this.studyLoading.set(false);
        },
        error: (err) => {
          this.error.set(parseApiError(err).message);
          if (reset) this.studyQueue.set([]);
          this.studyLoading.set(false);
        },
      });
  }
}

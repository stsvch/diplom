import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { parseApiError } from '../../../core/models/api-error.model';
import { UserRole } from '../../../core/models/user.model';
import { AuthService } from '../../../core/services/auth.service';
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
  tags: string;
}

@Component({
  selector: 'app-glossary-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
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
    if (!this.editor.courseId) {
      this.editor.courseId = this.selectedCourseId() || this.courses()[0]?.id || '';
    }
  }

  editWord(word: DictionaryWordDto): void {
    this.editingWordId.set(word.id);
    this.editor = {
      courseId: word.courseId,
      term: word.term,
      translation: word.translation,
      definition: word.definition ?? '',
      example: word.example ?? '',
      tags: word.tags.join(', '),
    };
  }

  cancelEdit(): void {
    this.editingWordId.set(null);
    this.editor = this.createEmptyEditor();
  }

  saveWord(): void {
    this.error.set(null);
    this.saving.set(true);

    const payload: UpsertDictionaryWordRequest = {
      courseId: this.editor.courseId,
      term: this.editor.term,
      translation: this.editor.translation,
      definition: this.editor.definition.trim() || null,
      example: this.editor.example.trim() || null,
      tags: this.editor.tags
        .split(',')
        .map((tag) => tag.trim())
        .filter(Boolean),
    };

    const request$ = this.editingWordId()
      ? this.glossaryService.updateWord(this.editingWordId()!, payload)
      : this.glossaryService.createWord(payload);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.cancelEdit();
        this.reloadWords();
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.saving.set(false);
      },
    });
  }

  deleteWord(wordId: string): void {
    this.error.set(null);
    this.deletingWordId.set(wordId);

    this.glossaryService.deleteWord(wordId).subscribe({
      next: () => {
        this.deletingWordId.set(null);
        if (this.editingWordId() === wordId) {
          this.cancelEdit();
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
    if (!current) {
      return;
    }

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
      tags: '',
    };
  }

  private advanceStudyQueue(updated: DictionaryWordDto): void {
    const currentIndex = this.studyIndex();
    const queue = [...this.studyQueue()];

    if (currentIndex < 0 || currentIndex >= queue.length) {
      return;
    }

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
    if (!this.studyMode()) {
      return;
    }

    if (!force && this.studyQueue().length >= 4) {
      return;
    }

    if (this.studyLoading()) {
      return;
    }

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
          if (reset) {
            this.studyQueue.set([]);
          }
          this.studyLoading.set(false);
        },
      });
  }
}

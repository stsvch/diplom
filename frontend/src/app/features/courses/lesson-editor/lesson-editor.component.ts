import {
  Component,
  inject,
  signal,
  OnInit,
  OnDestroy,
  computed,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, takeUntil, forkJoin, from, concatMap, toArray } from 'rxjs';
import { LucideAngularModule } from 'lucide-angular';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import {
  BlockHostComponent,
  BlockInserterComponent,
  BlockEditorHostComponent,
} from '../../content/components';
import {
  ContentService,
  BlockAttemptsService,
  LESSON_TEMPLATES,
  LessonTemplate,
} from '../../content/services';
import {
  LessonBlockDto,
  LessonBlockData,
  LessonBlockSettings,
  LessonBlockType,
  defaultBlockData,
  defaultSettings,
} from '../../content/models';
import { CoursesService } from '../services/courses.service';
import { LessonDto, LessonLayout } from '../models/course.model';

type SaveStatus = 'idle' | 'saving' | 'saved' | 'error';

interface BlockDraft {
  block: LessonBlockDto;
  dirty: boolean;
}

@Component({
  selector: 'app-lesson-editor',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    LucideAngularModule,
    DragDropModule,
    BlockHostComponent,
    BlockInserterComponent,
    BlockEditorHostComponent,
  ],
  templateUrl: './lesson-editor.component.html',
  styleUrl: './lesson-editor.component.scss',
})
export class LessonEditorComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly contentService = inject(ContentService);
  private readonly coursesService = inject(CoursesService);
  private readonly attemptsService = inject(BlockAttemptsService);
  private readonly toast = inject(ToastService);

  private readonly destroy$ = new Subject<void>();
  private readonly autoSaveBlock$ = new Subject<string>();
  private readonly autoSaveLesson$ = new Subject<void>();

  lessonId = signal('');
  lesson = signal<LessonDto | null>(null);
  loading = signal(true);
  drafts = signal<BlockDraft[]>([]);
  saveStatus = signal<SaveStatus>('idle');
  lastError = signal<string | null>(null);
  showLessonSettings = signal(false);
  showTemplates = signal(false);

  templates = LESSON_TEMPLATES;

  blocksCount = computed(() => this.drafts().length);

  ngOnInit() {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      const id = params.get('id');
      if (!id) {
        this.router.navigate(['/teacher/courses']);
        return;
      }
      this.lessonId.set(id);
      this.loadAll();
    });

    this.autoSaveBlock$
      .pipe(debounceTime(1500), takeUntil(this.destroy$))
      .subscribe((blockId) => this.persistBlock(blockId));

    this.autoSaveLesson$
      .pipe(debounceTime(1000), takeUntil(this.destroy$))
      .subscribe(() => this.persistLesson());
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadAll() {
    this.loading.set(true);
    forkJoin({
      lesson: this.coursesService.getLessonById(this.lessonId()),
      blocks: this.contentService.getByLesson(this.lessonId()),
    }).subscribe({
      next: ({ lesson, blocks }) => {
        this.lesson.set(lesson);
        this.drafts.set(blocks.map((b) => ({ block: b, dirty: false })));
        this.loading.set(false);
        if (blocks.length === 0) this.showTemplates.set(true);
      },
      error: (err) => {
        this.loading.set(false);
        const e = err.error as ApiError | undefined;
        this.toast.error(e?.message ?? 'Не удалось загрузить урок');
      },
    });
  }

  onTitleChange(value: string) {
    const l = this.lesson();
    if (!l) return;
    this.lesson.set({ ...l, title: value });
    this.autoSaveLesson$.next();
  }

  onLayoutChange(layout: LessonLayout) {
    const l = this.lesson();
    if (!l) return;
    this.lesson.set({ ...l, layout });
    this.autoSaveLesson$.next();
  }

  private persistLesson() {
    const l = this.lesson();
    if (!l) return;
    this.coursesService
      .updateLesson(l.id, { title: l.title, description: l.description, duration: l.duration, layout: l.layout })
      .subscribe({
        error: (err) => {
          const e = err.error as ApiError | undefined;
          this.toast.error(e?.message ?? 'Не удалось сохранить настройки урока');
        },
      });
  }

  applyTemplate(tpl: LessonTemplate) {
    if (tpl.blocks.length === 0) {
      this.showTemplates.set(false);
      return;
    }
    from(tpl.blocks)
      .pipe(
        concatMap((b) =>
          this.contentService.create({
            lessonId: this.lessonId(),
            type: b.type,
            data: b.data,
            settings: b.settings,
          }),
        ),
        toArray(),
      )
      .subscribe({
        next: (created) => {
          this.drafts.set(created.map((b) => ({ block: b, dirty: false })));
          this.showTemplates.set(false);
          this.toast.success('Шаблон применён');
        },
        error: (err) => {
          const e = err.error as ApiError | undefined;
          this.toast.error(e?.message ?? 'Не удалось применить шаблон');
        },
      });
  }

  insertBlock(type: LessonBlockType, afterIndex: number) {
    const data = defaultBlockData(type);
    const settings = defaultSettings();

    this.contentService
      .create({ lessonId: this.lessonId(), type, data, settings })
      .subscribe({
        next: (created) => {
          const next = [...this.drafts()];
          next.splice(afterIndex + 1, 0, { block: created, dirty: false });
          this.drafts.set(next);
          if (afterIndex + 1 < next.length - 1) {
            this.persistOrder();
          }
        },
        error: (err) => {
          const e = err.error as ApiError | undefined;
          this.toast.error(e?.message ?? 'Не удалось создать блок');
        },
      });
  }

  onDataChange(blockId: string, data: LessonBlockData) {
    this.drafts.update((arr) =>
      arr.map((d) => (d.block.id === blockId ? { block: { ...d.block, data }, dirty: true } : d)),
    );
    this.saveStatus.set('idle');
    this.autoSaveBlock$.next(blockId);
  }

  onSettingsChange(blockId: string, settings: LessonBlockSettings) {
    this.drafts.update((arr) =>
      arr.map((d) => (d.block.id === blockId ? { block: { ...d.block, settings }, dirty: true } : d)),
    );
    this.saveStatus.set('idle');
    this.autoSaveBlock$.next(blockId);
  }

  private persistBlock(blockId: string) {
    const draft = this.drafts().find((d) => d.block.id === blockId);
    if (!draft || !draft.dirty) return;

    this.saveStatus.set('saving');
    this.contentService
      .update(blockId, { data: draft.block.data, settings: draft.block.settings })
      .subscribe({
        next: (updated) => {
          this.drafts.update((arr) =>
            arr.map((d) => (d.block.id === blockId ? { block: updated, dirty: false } : d)),
          );
          this.saveStatus.set('saved');
          this.lastError.set(null);
          setTimeout(() => {
            if (this.saveStatus() === 'saved') this.saveStatus.set('idle');
          }, 2000);
        },
        error: (err) => {
          this.saveStatus.set('error');
          const e = err.error as ApiError | undefined;
          this.lastError.set(e?.message ?? 'Не удалось сохранить блок');
        },
      });
  }

  removeBlock(blockId: string) {
    if (!confirm('Удалить этот блок?')) return;
    this.contentService.delete(blockId).subscribe({
      next: () => {
        this.drafts.update((arr) => arr.filter((d) => d.block.id !== blockId));
      },
      error: (err) => {
        const e = err.error as ApiError | undefined;
        this.toast.error(e?.message ?? 'Не удалось удалить блок');
      },
    });
  }

  duplicateBlock(blockId: string) {
    const source = this.drafts().find((d) => d.block.id === blockId);
    if (!source) return;
    this.contentService
      .create({
        lessonId: this.lessonId(),
        type: source.block.type,
        data: source.block.data,
        settings: { ...source.block.settings },
      })
      .subscribe({
        next: (created) => {
          const idx = this.drafts().findIndex((d) => d.block.id === blockId);
          const next = [...this.drafts()];
          next.splice(idx + 1, 0, { block: created, dirty: false });
          this.drafts.set(next);
          this.persistOrder();
        },
        error: (err) => {
          const e = err.error as ApiError | undefined;
          this.toast.error(e?.message ?? 'Не удалось дублировать');
        },
      });
  }

  onDrop(event: CdkDragDrop<BlockDraft[]>) {
    if (event.previousIndex === event.currentIndex) return;
    const arr = [...this.drafts()];
    moveItemInArray(arr, event.previousIndex, event.currentIndex);
    this.drafts.set(arr);
    this.persistOrder();
  }

  private persistOrder() {
    const ids = this.drafts().map((d) => d.block.id);
    this.contentService.reorder(this.lessonId(), ids).subscribe({
      error: () => this.toast.error('Не удалось сохранить порядок'),
    });
  }
}

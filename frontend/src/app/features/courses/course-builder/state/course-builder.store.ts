import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject, debounceTime, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CourseBuilderService } from '../services/course-builder.service';
import { CoursesService } from '../../services/courses.service';
import { environment } from '../../../../../environments/environment';
import {
  CourseBuilderDto,
  CourseBuilderItemDto,
  CourseBuilderSectionDto,
  CourseItemType,
  Selection,
} from '../models/course-builder.model';
import { ToastService } from '../../../../shared/components/toast/toast.service';

export type SaveStatus = 'idle' | 'saving' | 'saved' | 'error';

interface CourseInfoPatch {
  title?: string;
  description?: string;
  imageUrl?: string;
  tags?: string;
  isFree?: boolean;
  price?: number | null;
  deadline?: string | null;
  hasGrading?: boolean;
  hasCertificate?: boolean;
}

@Injectable()
export class CourseBuilderStore {
  private readonly api = inject(CourseBuilderService);
  private readonly coursesApi = inject(CoursesService);
  private readonly http = inject(HttpClient);
  private readonly toast = inject(ToastService);
  private readonly base = environment.apiUrl;

  // ── State ────────────────────────────────────────────
  readonly courseId = signal<string>('');
  readonly builder = signal<CourseBuilderDto | null>(null);
  readonly selection = signal<Selection>({ kind: 'none' });
  readonly loading = signal<boolean>(false);
  readonly saveStatus = signal<SaveStatus>('idle');
  readonly isDirty = signal<boolean>(false);
  readonly lastSaved = signal<Date | null>(null);
  readonly showOnboarding = signal<boolean>(false);

  // ── Computed ────────────────────────────────────────
  readonly course = computed(() => this.builder()?.course ?? null);
  readonly sections = computed<CourseBuilderSectionDto[]>(() => this.builder()?.sections ?? []);
  readonly unsectioned = computed<CourseBuilderItemDto[]>(
    () => this.builder()?.unsectionedItems ?? [],
  );
  readonly readiness = computed(() => this.builder()?.readiness ?? null);

  readonly allItems = computed<CourseBuilderItemDto[]>(() => {
    const fromSections = this.sections().flatMap((s) => s.items);
    return [...fromSections, ...this.unsectioned()];
  });

  readonly totalItems = computed(() => this.allItems().length);

  readonly selectedItem = computed<CourseBuilderItemDto | null>(() => {
    const sel = this.selection();
    if (sel.kind !== 'item') return null;
    return this.allItems().find((i) => i.sourceId === sel.itemId) ?? null;
  });

  readonly selectedSection = computed<CourseBuilderSectionDto | null>(() => {
    const item = this.selectedItem();
    if (!item || !item.sectionId) return null;
    return this.sections().find((s) => s.id === item.sectionId) ?? null;
  });

  readonly progressPercent = computed(() => {
    const r = this.readiness();
    if (!r) return 0;
    return Math.round(Number(r.readyPercent) || 0);
  });

  // ── Save scheduling (auto-save для course info) ─────
  private readonly courseInfoSave$ = new Subject<CourseInfoPatch>();

  constructor() {
    this.courseInfoSave$
      .pipe(
        debounceTime(800),
        switchMap((patch) => {
          this.saveStatus.set('saving');
          const c = this.course();
          if (!c) return [];
          return this.coursesApi.updateCourse(c.id, {
            disciplineId: c.disciplineId,
            title: patch.title ?? c.title,
            description: patch.description ?? c.description,
            price: patch.price ?? c.price,
            isFree: patch.isFree ?? c.isFree,
            orderType: c.orderType as any,
            hasGrading: patch.hasGrading ?? c.hasGrading,
            level: c.level as any,
            imageUrl: patch.imageUrl ?? c.imageUrl,
            tags: patch.tags ?? c.tags,
            hasCertificate: patch.hasCertificate ?? c.hasCertificate,
            deadline: patch.deadline ?? c.deadline,
          } as any);
        }),
        takeUntilDestroyed(),
      )
      .subscribe({
        next: () => {
          this.saveStatus.set('saved');
          this.lastSaved.set(new Date());
          this.isDirty.set(false);
        },
        error: () => {
          this.saveStatus.set('error');
          this.toast.error('Не удалось сохранить курс');
        },
      });
  }

  // ── Actions ────────────────────────────────────────

  load(courseId: string): void {
    this.courseId.set(courseId);
    this.loading.set(true);
    this.api.getBuilder(courseId).subscribe({
      next: (data) => {
        this.builder.set(data);
        this.loading.set(false);
        this.isDirty.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toast.error('Не удалось загрузить курс');
      },
    });
  }

  refresh(): void {
    const id = this.courseId();
    if (!id) return;
    this.api.getBuilder(id).subscribe({
      next: (data) => this.builder.set(data),
    });
  }

  setSelection(sel: Selection): void {
    this.selection.set(sel);
  }

  // ── Course info ──

  patchCourseInfo(patch: CourseInfoPatch): void {
    const b = this.builder();
    if (!b) return;
    this.builder.set({ ...b, course: { ...b.course, ...patch } as any });
    this.isDirty.set(true);
    this.courseInfoSave$.next(patch);
  }

  // ── Sections ──

  addSection(): void {
    const id = this.courseId();
    if (!id) return;
    this.coursesApi.createModule(id, { title: 'Новый раздел' }).subscribe({
      next: () => this.refresh(),
      error: () => this.toast.error('Не удалось создать раздел'),
    });
  }

  updateSection(sectionId: string, title: string): void {
    this.coursesApi.updateModule(sectionId, { title }).subscribe({
      next: () => {
        const b = this.builder();
        if (!b) return;
        this.builder.set({
          ...b,
          sections: b.sections.map((s) => (s.id === sectionId ? { ...s, title } : s)),
        });
      },
      error: () => this.toast.error('Не удалось обновить раздел'),
    });
  }

  deleteSection(sectionId: string): void {
    this.coursesApi.deleteModule(sectionId).subscribe({
      next: () => this.refresh(),
      error: () => this.toast.error('Не удалось удалить раздел'),
    });
  }

  reorderSections(orderedIds: string[]): void {
    const id = this.courseId();
    if (!id) return;
    this.coursesApi.reorderModules(id, orderedIds).subscribe({
      next: () => this.refresh(),
      error: () => this.toast.error('Не удалось пересортировать разделы'),
    });
  }

  // ── Items ──

  addItem(sectionId: string | null, type: CourseItemType): void {
    const courseId = this.courseId();
    if (!courseId) return;

    if (type === 'Lesson') {
      if (!sectionId) {
        this.toast.error('Урок нужно создавать внутри раздела');
        return;
      }
      this.coursesApi.createLesson(sectionId, { title: 'Новый урок' }).subscribe({
        next: (lesson) => {
          this.refresh();
          setTimeout(() => {
            this.selection.set({ kind: 'item', sectionId, itemId: lesson.id });
          }, 200);
        },
        error: () => this.toast.error('Не удалось создать урок'),
      });
      return;
    }

    if (type === 'Test') {
      this.http
        .post<{ id: string }>(`${this.base}/tests`, {
          title: 'Новый тест',
          courseId,
          maxAttempts: 3,
        })
        .subscribe({
          next: (created) => {
            this.refresh();
            setTimeout(() => {
              this.selection.set({ kind: 'item', sectionId, itemId: created.id });
            }, 200);
          },
          error: () => this.toast.error('Не удалось создать тест'),
        });
      return;
    }

    if (type === 'Assignment') {
      this.http
        .post<{ id: string }>(`${this.base}/assignments`, {
          title: 'Новое задание',
          courseId,
          description: '',
          maxScore: 100,
        })
        .subscribe({
          next: (created) => {
            this.refresh();
            setTimeout(() => {
              this.selection.set({ kind: 'item', sectionId, itemId: created.id });
            }, 200);
          },
          error: () => this.toast.error('Не удалось создать задание'),
        });
      return;
    }

    if (type === 'LiveSession') {
      const start = new Date();
      start.setDate(start.getDate() + 1);
      start.setMinutes(0, 0, 0);
      const end = new Date(start.getTime() + 60 * 60 * 1000);
      this.http
        .post<{ id: string }>(`${this.base}/schedule/slots`, {
          courseId,
          title: 'Новое live-занятие',
          startTime: start.toISOString(),
          endTime: end.toISOString(),
          isGroupSession: true,
          maxStudents: 30,
        })
        .subscribe({
          next: (created) => {
            this.refresh();
            setTimeout(() => {
              this.selection.set({ kind: 'item', sectionId, itemId: created.id });
            }, 200);
          },
          error: () => this.toast.error('Не удалось создать live-занятие'),
        });
      return;
    }

    if (type === 'Resource' || type === 'ExternalLink') {
      this.api
        .createStandaloneItem(courseId, {
          type,
          sectionId,
          title: type === 'Resource' ? 'Новый материал' : 'Новая ссылка',
          url: type === 'ExternalLink' ? 'https://' : null,
        })
        .subscribe({
          next: (item) => {
            this.refresh();
            setTimeout(() => {
              this.selection.set({ kind: 'item', sectionId, itemId: item.sourceId });
            }, 200);
          },
          error: (err) => {
            this.toast.error(err?.error?.message ?? 'Не удалось создать элемент');
          },
        });
    }
  }

  removeItem(item: CourseBuilderItemDto): void {
    const courseId = this.courseId();

    if (item.type === 'Resource' || item.type === 'ExternalLink') {
      if (!item.courseItemId) return;
      this.api.deleteStandaloneItem(courseId, item.courseItemId).subscribe({
        next: () => {
          this.selection.set({ kind: 'none' });
          this.refresh();
        },
        error: () => this.toast.error('Не удалось удалить элемент'),
      });
      return;
    }

    let url: string | null = null;
    if (item.type === 'Lesson') url = `${this.base}/lessons/${item.sourceId}`;
    else if (item.type === 'Test') url = `${this.base}/tests/${item.sourceId}`;
    else if (item.type === 'Assignment') url = `${this.base}/assignments/${item.sourceId}`;
    else if (item.type === 'LiveSession') url = `${this.base}/schedule/slots/${item.sourceId}`;
    if (!url) return;

    this.http.delete(url).subscribe({
      next: () => {
        this.selection.set({ kind: 'none' });
        this.refresh();
      },
      error: () => this.toast.error('Не удалось удалить элемент'),
    });
  }

  updateItemTitle(item: CourseBuilderItemDto, title: string): void {
    const courseId = this.courseId();
    this.patchItemLocally(item.sourceId, { title });

    if (item.type === 'Lesson') {
      this.http.put(`${this.base}/lessons/${item.sourceId}`, { title }).subscribe();
    } else if (item.type === 'Test') {
      this.http.put(`${this.base}/tests/${item.sourceId}`, { title }).subscribe();
    } else if (item.type === 'Assignment') {
      this.http.put(`${this.base}/assignments/${item.sourceId}`, { title }).subscribe();
    } else if (item.type === 'LiveSession') {
      this.http.put(`${this.base}/schedule/slots/${item.sourceId}`, { title }).subscribe();
    } else if (item.courseItemId) {
      this.api
        .updateStandaloneItem(courseId, item.courseItemId, {
          title,
          description: item.description,
          url: item.url,
          attachmentId: item.attachmentId,
          resourceKind: item.resourceKind,
        })
        .subscribe();
    }
  }

  setItemRequired(item: CourseBuilderItemDto, required: boolean): void {
    const courseId = this.courseId();
    if (!item.courseItemId) return;
    this.patchItemLocally(item.sourceId, { isRequired: required });
    this.api
      .updateItemMetadata(courseId, item.courseItemId, {
        isRequired: required,
        points: item.points,
        availableFrom: item.availableFrom,
        deadline: item.deadline,
        status: item.status,
      })
      .subscribe();
  }

  patchItemLocally(itemSourceId: string, patch: Partial<CourseBuilderItemDto>): void {
    const b = this.builder();
    if (!b) return;
    const transform = (i: CourseBuilderItemDto): CourseBuilderItemDto =>
      i.sourceId === itemSourceId ? { ...i, ...patch } : i;
    this.builder.set({
      ...b,
      sections: b.sections.map((s) => ({ ...s, items: s.items.map(transform) })),
      unsectionedItems: b.unsectionedItems.map(transform),
    });
  }

  moveItem(itemId: string, targetSectionId: string | null, orderIndex: number): void {
    const courseId = this.courseId();
    const target = this.allItems().find((i) => i.sourceId === itemId);
    if (!target?.courseItemId) return;
    this.api
      .moveItem(courseId, target.courseItemId, { sectionId: targetSectionId, orderIndex })
      .subscribe({
        next: () => this.refresh(),
        error: () => this.toast.error('Не удалось переместить элемент'),
      });
  }

  /** Пересортировать items внутри одной секции (по списку courseItemId в нужном порядке) */
  reorderItems(sectionId: string | null, courseItemIds: string[]): void {
    const courseId = this.courseId();
    if (courseItemIds.length === 0) return;
    this.api
      .reorderItems(courseId, { sectionId, itemIds: courseItemIds })
      .subscribe({
        next: () => this.refresh(),
        error: () => {
          this.toast.error('Не удалось пересортировать элементы');
          this.refresh();
        },
      });
  }

  // ── Publish ──

  publish(force = false): void {
    const courseId = this.courseId();
    this.coursesApi.publishCourse(courseId, force).subscribe({
      next: (res) => {
        if (res.success) {
          this.toast.success(res.message ?? 'Курс опубликован');
          this.refresh();
        } else {
          this.toast.error(res.message ?? 'Курс не готов к публикации');
        }
      },
      error: () => this.toast.error('Не удалось опубликовать курс'),
    });
  }
}

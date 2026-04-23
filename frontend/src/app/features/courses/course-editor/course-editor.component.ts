import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import { LucideAngularModule } from 'lucide-angular';
import { CoursesService, PublishIssue } from '../services/courses.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { CourseDetailDto, CourseModuleDetailDto, LessonDetailDto } from '../models/course.model';
import { ModuleTreeComponent, ModuleTreeEvent } from './module-tree.component';
import { CourseSettingsFormComponent, CourseSettingsValue } from '../shared/course-settings-form.component';
import { PublishChecklistComponent } from './publish-checklist.component';

type SaveStatus = 'idle' | 'saving' | 'saved' | 'error';

@Component({
  selector: 'app-course-editor',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule, ModuleTreeComponent, CourseSettingsFormComponent, PublishChecklistComponent],
  templateUrl: './course-editor.component.html',
  styleUrl: './course-editor.component.scss',
})
export class CourseEditorComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly coursesService = inject(CoursesService);
  private readonly toast = inject(ToastService);

  private readonly destroy$ = new Subject<void>();
  private readonly save$ = new Subject<void>();

  courseId = signal<string>('');
  course = signal<CourseDetailDto | null>(null);
  loading = signal(true);
  saveStatus = signal<SaveStatus>('idle');
  showSettings = signal(false);
  settingsValue = signal<CourseSettingsValue | null>(null);

  publishDialogOpen = signal(false);
  publishIssues = signal<PublishIssue[]>([]);
  publishing = signal(false);

  titleDraft = signal('');
  modules = computed<CourseModuleDetailDto[]>(() => this.course()?.modules ?? []);

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe((p) => {
      const id = p.get('id');
      if (!id) {
        this.router.navigate(['/teacher/courses']);
        return;
      }
      this.courseId.set(id);
      this.loadCourse();
    });

    this.save$
      .pipe(debounceTime(800), takeUntil(this.destroy$))
      .subscribe(() => this.persistCourse());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadCourse(): void {
    this.loading.set(true);
    this.coursesService.getCourseById(this.courseId()).subscribe({
      next: (c) => {
        this.course.set(c);
        this.titleDraft.set(c.title);
        this.settingsValue.set({
          deadline: c.deadline ? c.deadline.substring(0, 10) : '',
          hasCertificate: c.hasCertificate ?? false,
          orderType: c.orderType ?? 'Sequential',
          hasGrading: c.hasGrading ?? false,
        });
        this.loading.set(false);
      },
      error: (err: ApiError) => {
        this.loading.set(false);
        this.toast.error(err.message);
      },
    });
  }

  onTitleChange(value: string): void {
    this.titleDraft.set(value);
    this.saveStatus.set('idle');
    this.save$.next();
  }

  onSettingsChange(v: CourseSettingsValue): void {
    this.settingsValue.set(v);
    this.saveStatus.set('idle');
    this.save$.next();
  }

  private persistCourse(): void {
    const c = this.course();
    const s = this.settingsValue();
    if (!c || !s) return;

    this.saveStatus.set('saving');
    const payload: any = {
      title: this.titleDraft(),
      description: c.description,
      disciplineId: c.disciplineId,
      level: c.level,
      isFree: c.isFree,
      price: c.price ?? null,
      imageUrl: c.imageUrl ?? null,
      tags: c.tags ?? null,
      orderType: s.orderType,
      hasGrading: s.hasGrading,
      hasCertificate: s.hasCertificate,
      deadline: s.deadline ? new Date(s.deadline).toISOString() : null,
    };

    this.coursesService.updateCourse(this.courseId(), payload).subscribe({
      next: (updated) => {
        this.course.set({ ...this.course()!, ...updated, modules: this.course()!.modules });
        this.saveStatus.set('saved');
        setTimeout(() => {
          if (this.saveStatus() === 'saved') this.saveStatus.set('idle');
        }, 1500);
      },
      error: (err: ApiError) => {
        this.saveStatus.set('error');
        this.toast.error(err.message);
      },
    });
  }

  addModule(): void {
    const courseId = this.courseId();
    this.coursesService.createModule(courseId, { title: 'Новый модуль' }).subscribe({
      next: () => this.reloadModules(),
      error: (err: ApiError) => this.toast.error(err.message),
    });
  }

  onTreeEvent(event: ModuleTreeEvent): void {
    switch (event.kind) {
      case 'rename-module':
        this.coursesService
          .updateModule(event.moduleId, { title: event.title })
          .subscribe({ error: (err: ApiError) => this.toast.error(err.message), complete: () => this.reloadModules() });
        break;

      case 'delete-module':
        this.coursesService.deleteModule(event.moduleId).subscribe({
          next: () => this.reloadModules(),
          error: (err: ApiError) => this.toast.error(err.message),
        });
        break;

      case 'add-lesson':
        this.coursesService
          .createLesson(event.moduleId, { title: 'Новый урок' })
          .subscribe({
            next: () => this.reloadModules(),
            error: (err: ApiError) => this.toast.error(err.message),
          });
        break;

      case 'rename-lesson':
        this.coursesService
          .updateLesson(event.lessonId, { title: event.title })
          .subscribe({
            error: (err: ApiError) => this.toast.error(err.message),
            complete: () => this.reloadModules(),
          });
        break;

      case 'delete-lesson':
        this.coursesService.deleteLesson(event.lessonId).subscribe({
          next: () => this.reloadModules(),
          error: (err: ApiError) => this.toast.error(err.message),
        });
        break;

      case 'duplicate-lesson': {
        const sourceLesson = this.findLesson(event.lessonId);
        if (!sourceLesson) return;
        this.coursesService
          .createLesson(event.moduleId, {
            title: sourceLesson.title + ' (копия)',
            description: sourceLesson.description,
            duration: sourceLesson.duration,
            layout: sourceLesson.layout,
          })
          .subscribe({
            next: () => {
              this.toast.info('Дублирован только каркас урока. Блоки скопируйте вручную.');
              this.reloadModules();
            },
            error: (err: ApiError) => this.toast.error(err.message),
          });
        break;
      }

      case 'reorder-modules':
        this.coursesService.reorderModules(this.courseId(), event.orderedIds).subscribe({
          error: (err: ApiError) => {
            this.toast.error(err.message);
            this.reloadModules();
          },
        });
        break;

      case 'reorder-lessons':
        this.coursesService.reorderLessons(event.moduleId, event.orderedIds).subscribe({
          error: (err: ApiError) => {
            this.toast.error(err.message);
            this.reloadModules();
          },
        });
        break;

      case 'move-lesson':
        this.coursesService
          .updateLesson(event.lessonId, { moduleId: event.toModuleId } as any)
          .subscribe({
            next: () => {
              const toModule = this.modules().find((m) => m.id === event.toModuleId);
              if (toModule) {
                const orderedIds = toModule.lessons.map((l) => l.id);
                this.coursesService.reorderLessons(event.toModuleId, orderedIds).subscribe({
                  next: () => this.reloadModules(),
                  error: (err: ApiError) => {
                    this.toast.error(err.message);
                    this.reloadModules();
                  },
                });
              } else {
                this.reloadModules();
              }
            },
            error: (err: ApiError) => {
              this.toast.error(err.message);
              this.reloadModules();
            },
          });
        break;
    }
  }

  private findLesson(id: string): LessonDetailDto | null {
    for (const m of this.modules()) {
      const l = m.lessons.find((x) => x.id === id);
      if (l) return l;
    }
    return null;
  }

  private reloadModules(): void {
    this.coursesService.getCourseById(this.courseId()).subscribe({
      next: (c) => {
        const current = this.course();
        if (current) this.course.set({ ...current, modules: c.modules });
      },
    });
  }

  publish(): void {
    this.runPublish(false);
  }

  onConfirmPublish(force: boolean): void {
    this.runPublish(force);
  }

  closePublishDialog(): void {
    if (this.publishing()) return;
    this.publishDialogOpen.set(false);
  }

  private runPublish(force: boolean): void {
    this.publishing.set(true);
    this.coursesService.publishCourse(this.courseId(), force).subscribe({
      next: (validation) => {
        this.publishing.set(false);
        if (validation.success) {
          this.publishDialogOpen.set(false);
          this.publishIssues.set([]);
          this.toast.success(validation.message || 'Курс опубликован');
          this.loadCourse();
        } else {
          this.publishIssues.set(validation.issues ?? []);
          this.publishDialogOpen.set(true);
        }
      },
      error: (err: ApiError) => {
        this.publishing.set(false);
        this.toast.error(err.message);
      },
    });
  }

  archive(): void {
    if (!confirm('Архивировать курс? Он исчезнет из каталога.')) return;
    this.coursesService.archiveCourse(this.courseId()).subscribe({
      next: () => {
        this.toast.success('Курс архивирован');
        this.loadCourse();
      },
      error: (err: ApiError) => this.toast.error(err.message),
    });
  }

  openPreview(): void {
    window.open(`/teacher/courses/${this.courseId()}/preview`, '_blank');
  }
}

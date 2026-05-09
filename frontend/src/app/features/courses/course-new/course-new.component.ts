import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import {
  LucideAngularModule,
  ChevronLeft,
  Sparkles,
  Plus,
  Layers,
  ArrowRight,
  BookOpen,
  ClipboardCheck,
  FileEdit,
} from 'lucide-angular';
import { concatMap, from, of, switchMap, toArray } from 'rxjs';
import { CoursesService } from '../services/courses.service';
import { DisciplinesService } from '../services/disciplines.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { COURSE_TEMPLATES, CourseTemplate } from '../services/course-templates';

type CreateMode = 'empty' | 'template';

@Component({
  selector: 'app-course-new',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule],
  templateUrl: './course-new.component.html',
  styleUrl: './course-new.component.scss',
})
export class CourseNewComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly coursesService = inject(CoursesService);
  private readonly disciplinesService = inject(DisciplinesService);
  private readonly toast = inject(ToastService);

  readonly icons = {
    chevronLeft: ChevronLeft,
    sparkles: Sparkles,
    plus: Plus,
    layers: Layers,
    arrowRight: ArrowRight,
    bookOpen: BookOpen,
    clipboardCheck: ClipboardCheck,
    fileEdit: FileEdit,
  };

  readonly templates = COURSE_TEMPLATES;
  readonly templateIcons: Record<CourseTemplate['icon'], any> = {
    mini: BookOpen,
    tests: ClipboardCheck,
    homework: FileEdit,
  };

  readonly mode = signal<CreateMode>('empty');
  readonly templateId = signal<string | null>(null);
  readonly creating = signal(false);
  readonly disciplineId = signal<string | null>(null);
  readonly disciplinesError = signal<string | null>(null);

  readonly canStart = computed(() => {
    if (this.creating()) return false;
    if (this.disciplineId() === null) return false;
    if (this.mode() === 'empty') return true;
    return this.templateId() !== null;
  });

  readonly ctaLabel = computed(() => {
    const m = this.mode();
    if (m === 'empty') return 'Создать пустой курс';
    return this.templateId() ? 'Создать по шаблону' : 'Выберите шаблон';
  });

  ngOnInit(): void {
    this.disciplinesService.getAll().subscribe({
      next: (list) => {
        if (!list.length) {
          this.disciplinesError.set('В системе нет дисциплин. Обратитесь к администратору.');
          return;
        }
        this.disciplineId.set(list[0].id);
      },
      error: (err: ApiError) => {
        this.disciplinesError.set(err.message ?? 'Не удалось загрузить дисциплины');
      },
    });
  }

  selectMode(m: CreateMode): void {
    this.mode.set(m);
    if (m !== 'template') this.templateId.set(null);
  }

  selectTemplate(id: string): void {
    this.templateId.set(id);
  }

  start(): void {
    if (!this.canStart()) return;
    const mode = this.mode();

    if (mode === 'empty') {
      this.createCourseAndGo(null);
      return;
    }
    const tpl = COURSE_TEMPLATES.find((t) => t.id === this.templateId());
    this.createCourseAndGo(tpl ?? null);
  }

  private createCourseAndGo(template: CourseTemplate | null): void {
    const disciplineId = this.disciplineId();
    if (!disciplineId) {
      this.toast.error(this.disciplinesError() ?? 'Дисциплина не выбрана');
      return;
    }

    this.creating.set(true);

    const draft: any = {
      title: 'Новый курс',
      description: 'Описание появится здесь. Заполните это поле в редакторе курса.',
      disciplineId,
      level: 'Beginner',
      isFree: true,
      price: null,
      orderType: 'Sequential',
      hasGrading: false,
      hasCertificate: false,
      deadline: null,
      imageUrl: null,
    };

    this.coursesService.createCourse(draft).subscribe({
      next: (course) => {
        if (!template || template.modules.length === 0) {
          this.goToBuilder(course.id);
          return;
        }
        this.scaffoldTemplate(course.id, template);
      },
      error: (err: ApiError) => {
        this.creating.set(false);
        this.toast.error(err.message ?? 'Не удалось создать курс');
      },
    });
  }

  private scaffoldTemplate(courseId: string, tpl: CourseTemplate): void {
    from(tpl.modules)
      .pipe(
        concatMap((m) =>
          this.coursesService
            .createModule(courseId, { title: m.title, description: m.description })
            .pipe(
              switchMap((created) =>
                from(m.lessons).pipe(
                  concatMap((l) =>
                    this.coursesService.createLesson(created.id, {
                      title: l.title,
                      description: l.description,
                    }),
                  ),
                  toArray(),
                ),
              ),
            ),
        ),
        toArray(),
      )
      .subscribe({
        next: () => this.goToBuilder(courseId),
        error: (err: ApiError) => {
          this.toast.warning(
            'Курс создан, но не вся шаблонная структура наполнилась: ' + (err.message ?? ''),
          );
          this.goToBuilder(courseId);
        },
      });
  }

  private goToBuilder(courseId: string): void {
    this.creating.set(false);
    this.router.navigate(['/teacher/courses', courseId, 'builder']);
  }
}

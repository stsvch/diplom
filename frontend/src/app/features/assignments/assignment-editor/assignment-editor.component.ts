import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  ChevronLeft,
  Save,
  Loader2,
  FileEdit,
  Plus,
} from 'lucide-angular';
import { AssignmentsService } from '../services/assignments.service';
import { CoursesService } from '../../courses/services/courses.service';
import { CourseListDto } from '../../courses/models/course.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { RichTextEditorComponent } from '../../../shared/components/rich-text-editor/rich-text-editor.component';

@Component({
  selector: 'app-assignment-editor',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    LucideAngularModule,
    ButtonComponent,
    RichTextEditorComponent,
  ],
  templateUrl: './assignment-editor.component.html',
  styleUrl: './assignment-editor.component.scss',
})
export class AssignmentEditorComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly coursesService = inject(CoursesService);
  private readonly toastService = inject(ToastService);

  readonly ChevronLeftIcon = ChevronLeft;
  readonly SaveIcon = Save;
  readonly Loader2Icon = Loader2;
  readonly FileEditIcon = FileEdit;
  readonly PlusIcon = Plus;

  readonly loading = signal(false);
  readonly saving = signal(false);

  assignmentId = '';
  isNew = true;

  // Form fields
  readonly title = signal('');
  readonly description = signal('');
  readonly criteria = signal('');
  readonly deadline = signal('');
  readonly maxAttempts = signal<number | null>(null);
  readonly maxScore = signal<number>(100);
  readonly courseId = signal<string>('');
  readonly courses = signal<CourseListDto[]>([]);

  // Context: what lesson block created this
  blockId = '';

  ngOnInit(): void {
    this.assignmentId = this.route.snapshot.paramMap.get('id') ?? '';
    this.blockId = this.route.snapshot.queryParamMap.get('blockId') ?? '';
    this.isNew = !this.assignmentId || this.assignmentId === 'new';

    this.coursesService.getMyCourses().subscribe({
      next: (list) => this.courses.set(list),
    });

    if (!this.isNew) {
      this.loadAssignment();
    }
  }

  loadAssignment(): void {
    this.loading.set(true);
    this.assignmentsService.getAssignment(this.assignmentId).subscribe({
      next: (data) => {
        this.title.set(data.title);
        this.description.set(data.description ?? '');
        this.criteria.set(data.criteria ?? '');
        this.deadline.set(data.deadline ? data.deadline.slice(0, 16) : '');
        this.maxAttempts.set(data.maxAttempts ?? null);
        this.maxScore.set(data.maxScore);
        this.courseId.set(data.courseId ?? '');
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  save(): void {
    if (!this.title().trim()) {
      this.toastService.error('Введите название задания');
      return;
    }
    if (!this.courseId()) {
      this.toastService.error('Выберите курс');
      return;
    }
    if (!this.maxScore() || this.maxScore() < 1) {
      this.toastService.error('Укажите максимальный балл (минимум 1)');
      return;
    }

    this.saving.set(true);

    const payload = {
      courseId: this.courseId(),
      title: this.title().trim(),
      description: this.description(),
      criteria: this.criteria() || undefined,
      deadline: this.deadline() ? new Date(this.deadline()).toISOString() : undefined,
      maxAttempts: this.maxAttempts() ?? undefined,
      maxScore: this.maxScore(),
    };

    if (this.isNew) {
      this.assignmentsService.createAssignment(payload).subscribe({
        next: (assignment) => {
          this.saving.set(false);
          this.toastService.success('Задание создано');
          // If we came from a lesson block, navigate back with the new assignment id
          if (this.blockId) {
            this.router.navigate(['/teacher/assignment', assignment.id, 'edit'], {
              queryParams: { blockId: this.blockId, created: '1' },
            });
          } else {
            this.router.navigate(['/teacher/assignment', assignment.id, 'edit']);
          }
        },
        error: (err) => {
          this.saving.set(false);
          this.toastService.error(parseApiError(err).message);
        },
      });
    } else {
      this.assignmentsService.updateAssignment(this.assignmentId, payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.toastService.success('Задание сохранено');
        },
        error: (err) => {
          this.saving.set(false);
          this.toastService.error(parseApiError(err).message);
        },
      });
    }
  }

  get pageTitle(): string {
    return this.isNew ? 'Новое задание' : 'Редактировать задание';
  }

  get backUrl(): string {
    return '/teacher/courses';
  }
}

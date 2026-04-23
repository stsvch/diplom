import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import {
  LucideAngularModule,
  Check,
  ChevronLeft,
  ChevronRight,
  Upload,
  Eye,
} from 'lucide-angular';
import { concatMap, from, of, toArray, catchError, switchMap } from 'rxjs';
import { CoursesService } from '../services/courses.service';
import { DisciplinesService } from '../services/disciplines.service';
import { FileService } from '../../../core/services/file.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { DisciplineDto } from '../models/course.model';
import { COURSE_TEMPLATES, CourseTemplate } from '../services/course-templates';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';

@Component({
  selector: 'app-create-course',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FormsModule,
    RouterLink,
    LucideAngularModule,
    ButtonComponent,
    CardComponent,
  ],
  templateUrl: './create-course.component.html',
  styleUrl: './create-course.component.scss',
})
export class CreateCourseComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly coursesService = inject(CoursesService);
  private readonly disciplinesService = inject(DisciplinesService);
  private readonly fileService = inject(FileService);
  private readonly toastService = inject(ToastService);

  coverUploading = signal(false);
  pendingCoverFile = signal<File | null>(null);
  coverPreviewUrl = signal<string | null>(null);
  pendingTemplate: CourseTemplate | null = null;

  readonly CheckIcon = Check;
  readonly ChevronLeftIcon = ChevronLeft;
  readonly ChevronRightIcon = ChevronRight;
  readonly UploadIcon = Upload;
  readonly EyeIcon = Eye;

  courseId = signal<string | null>(null);
  isEditMode = signal(false);

  readonly currentStep = signal(0);
  readonly saving = signal(false);
  readonly publishing = signal(false);
  readonly disciplines = signal<DisciplineDto[]>([]);

  readonly steps = [
    { label: 'Основное', icon: 'BookOpen' },
    { label: 'Настройки', icon: 'Settings' },
    { label: 'Публикация', icon: 'Eye' },
  ];

  readonly levels = [
    { value: 'Beginner', label: 'Начальный' },
    { value: 'Intermediate', label: 'Средний' },
    { value: 'Advanced', label: 'Продвинутый' },
  ];

  step1 = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(5)]],
    description: ['', [Validators.required, Validators.minLength(20)]],
    disciplineId: ['', Validators.required],
    level: ['Beginner', Validators.required],
    isFree: [true],
    price: [0],
    imageUrl: [''],
  });

  step2 = this.fb.group({
    deadline: [''],
    hasCertificate: [false],
    orderType: ['Sequential'],
    hasGrading: [false],
  });

  ngOnInit(): void {
    this.loadDisciplines();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.courseId.set(id);
      this.isEditMode.set(true);
      this.loadCourseForEdit(id);
      return;
    }

    const navState = this.router.getCurrentNavigation()?.extras?.state
      ?? (history.state as { templateId?: string } | undefined);
    const templateId = navState?.templateId;
    if (templateId) {
      const tpl = COURSE_TEMPLATES.find((t) => t.id === templateId);
      if (tpl && tpl.modules.length > 0) {
        this.pendingTemplate = tpl;
      }
    }
  }

  loadCourseForEdit(id: string): void {
    this.coursesService.getCourseById(id).subscribe({
      next: (course) => {
        this.step1.patchValue({
          title: course.title,
          description: course.description,
          disciplineId: course.disciplineId ?? '',
          level: course.level,
          isFree: course.isFree,
          price: course.price ?? 0,
          imageUrl: course.imageUrl ?? '',
        });
        this.step2.patchValue({
          deadline: course.deadline ? course.deadline.substring(0, 10) : '',
          hasCertificate: course.hasCertificate ?? false,
          orderType: course.orderType ?? 'Sequential',
          hasGrading: course.hasGrading ?? false,
        });
        if (course.imageUrl) this.coverPreviewUrl.set(course.imageUrl);
      },
      error: (err: ApiError) => {
        this.toastService.error(err.message);
      },
    });
  }

  loadDisciplines(): void {
    this.disciplinesService.getAll().subscribe({
      next: (data) => this.disciplines.set(data),
      error: (err: ApiError) => this.toastService.error(err.message),
    });
  }

  goToStep(step: number): void {
    if (step < this.currentStep()) {
      this.currentStep.set(step);
      return;
    }
    if (step >= 1 && this.step1.invalid) {
      this.step1.markAllAsTouched();
      this.toastService.warning('Заполните все обязательные поля');
      return;
    }
    this.currentStep.set(step);
  }

  nextStep(): void {
    this.goToStep(this.currentStep() + 1);
  }

  prevStep(): void {
    if (this.currentStep() > 0) this.currentStep.set(this.currentStep() - 1);
  }

  saveDraft(): void {
    this.saving.set(true);
    this.buildAndSave(false);
  }

  publishCourse(): void {
    this.publishing.set(true);
    this.buildAndSave(true);
  }

  private buildAndSave(publish: boolean): void {
    const v1 = this.step1.value;
    const v2 = this.step2.value;

    const deadline = v2.deadline ? new Date(v2.deadline).toISOString() : null;

    const courseData: any = {
      title: v1.title,
      description: v1.description,
      disciplineId: v1.disciplineId,
      level: v1.level,
      isFree: v1.isFree,
      price: v1.isFree ? null : v1.price,
      imageUrl: v1.imageUrl || null,
      orderType: v2.orderType,
      hasGrading: v2.hasGrading,
      hasCertificate: v2.hasCertificate,
      deadline,
    };

    const existingId = this.courseId();
    if (existingId) {
      this.coursesService.updateCourse(existingId, courseData).subscribe({
        next: () => this.afterCourseSaved(existingId, publish),
        error: (err: ApiError) => this.onSaveError(err),
      });
    } else {
      this.coursesService.createCourse(courseData).subscribe({
        next: (course) => this.afterCourseSaved(course.id, publish),
        error: (err: ApiError) => this.onSaveError(err),
      });
    }
  }

  private onSaveError(err: ApiError): void {
    this.saving.set(false);
    this.publishing.set(false);
    this.toastService.error(err.message);
  }

  private afterCourseSaved(courseId: string, publish: boolean): void {
    const pending = this.pendingCoverFile();
    const coverUpload$ = pending
      ? this.fileService.upload(pending, 'CourseCover', courseId).pipe(
          switchMap((att) => {
            this.pendingCoverFile.set(null);
            return this.coursesService.updateCourse(courseId, { imageUrl: att.fileUrl } as any);
          }),
          catchError(() => {
            this.toastService.warning('Курс сохранён, но обложку загрузить не удалось');
            return of(null);
          }),
        )
      : of(null);

    coverUpload$.subscribe({
      next: () => this.scaffoldTemplate(courseId, publish),
    });
  }

  private scaffoldTemplate(courseId: string, publish: boolean): void {
    const tpl = this.pendingTemplate;
    if (!tpl || this.isEditMode()) {
      this.finalizeCourse(courseId, publish);
      return;
    }

    this.pendingTemplate = null;
    from(tpl.modules)
      .pipe(
        concatMap((m) =>
          this.coursesService.createModule(courseId, { title: m.title, description: m.description }).pipe(
            switchMap((created) =>
              from(m.lessons).pipe(
                concatMap((l) =>
                  this.coursesService.createLesson(created.id, { title: l.title, description: l.description }),
                ),
                toArray(),
              ),
            ),
          ),
        ),
        toArray(),
      )
      .subscribe({
        next: () => this.finalizeCourse(courseId, publish),
        error: (err: ApiError) => {
          this.toastService.error('Не удалось создать шаблонную структуру: ' + err.message);
          this.finalizeCourse(courseId, publish);
        },
      });
  }

  private finalizeCourse(courseId: string, publish: boolean): void {
    if (publish) {
      this.coursesService.publishCourse(courseId).subscribe({
        next: (validation) => {
          this.saving.set(false);
          this.publishing.set(false);
          if (validation.success) {
            this.toastService.success(validation.message || 'Курс опубликован!');
          } else {
            this.toastService.warning('Курс сохранён как черновик — устраните ошибки в редакторе перед публикацией.');
          }
          this.router.navigate(['/teacher/courses', courseId, 'editor']);
        },
        error: (err: ApiError) => {
          this.publishing.set(false);
          this.toastService.error(err.message);
          this.router.navigate(['/teacher/courses', courseId, 'editor']);
        },
      });
    } else {
      this.saving.set(false);
      this.toastService.success('Черновик сохранён!');
      this.router.navigate(['/teacher/courses', courseId, 'editor']);
    }
  }

  get selectedDisciplineName(): string {
    const id = this.step1.value.disciplineId;
    return this.disciplines().find((d) => d.id === id)?.name ?? '—';
  }

  onCoverDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  onCoverDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const file = event.dataTransfer?.files?.[0];
    if (file) this.handleCoverFile(file);
  }

  onCoverFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) this.handleCoverFile(file);
    input.value = '';
  }

  removeCover(): void {
    const prev = this.coverPreviewUrl();
    if (prev && prev.startsWith('blob:')) URL.revokeObjectURL(prev);
    this.pendingCoverFile.set(null);
    this.coverPreviewUrl.set(null);
    this.step1.patchValue({ imageUrl: '' });
  }

  private handleCoverFile(file: File): void {
    if (!file.type.startsWith('image/')) {
      this.toastService.error('Нужен файл изображения (PNG, JPG).');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.toastService.error('Максимальный размер 5 МБ.');
      return;
    }

    const existingId = this.courseId();
    if (existingId) {
      this.uploadCoverFor(existingId, file);
      return;
    }

    const prev = this.coverPreviewUrl();
    if (prev && prev.startsWith('blob:')) URL.revokeObjectURL(prev);
    this.pendingCoverFile.set(file);
    this.coverPreviewUrl.set(URL.createObjectURL(file));
  }

  private uploadCoverFor(courseId: string, file: File): void {
    this.coverUploading.set(true);
    this.fileService.upload(file, 'CourseCover', courseId).subscribe({
      next: (att) => {
        this.step1.patchValue({ imageUrl: att.fileUrl });
        this.coverPreviewUrl.set(att.fileUrl);
        this.pendingCoverFile.set(null);
        this.coverUploading.set(false);
      },
      error: (err) => {
        this.coverUploading.set(false);
        this.toastService.error(err?.error?.message ?? 'Не удалось загрузить обложку');
      },
    });
  }

  getFieldError(form: FormGroup, field: string): string {
    const ctrl = form.get(field);
    if (!ctrl?.touched || !ctrl.invalid) return '';
    if (ctrl.hasError('required')) return 'Обязательное поле';
    if (ctrl.hasError('minlength')) return `Минимум ${ctrl.errors?.['minlength']?.requiredLength} символов`;
    return 'Некорректное значение';
  }
}

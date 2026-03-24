import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import {
  LucideAngularModule,
  Check,
  ChevronLeft,
  ChevronRight,
  Plus,
  Trash2,
  ArrowUp,
  ArrowDown,
  Upload,
  BookOpen,
  Settings,
  Eye,
  FileText,
  Edit2,
} from 'lucide-angular';
import { CoursesService } from '../services/courses.service';
import { DisciplinesService } from '../services/disciplines.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { DisciplineDto } from '../models/course.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';

interface WizardModule {
  id?: string;
  title: string;
  description: string;
  lessons: WizardLesson[];
  expanded: boolean;
}

interface WizardLesson {
  id?: string;
  title: string;
  description: string;
  duration: number | null;
}

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
    BadgeComponent,
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
  private readonly toastService = inject(ToastService);

  readonly CheckIcon = Check;
  readonly ChevronLeftIcon = ChevronLeft;
  readonly ChevronRightIcon = ChevronRight;
  readonly PlusIcon = Plus;
  readonly TrashIcon = Trash2;
  readonly ArrowUpIcon = ArrowUp;
  readonly ArrowDownIcon = ArrowDown;
  readonly UploadIcon = Upload;
  readonly BookOpenIcon = BookOpen;
  readonly SettingsIcon = Settings;
  readonly EyeIcon = Eye;
  readonly FileTextIcon = FileText;
  readonly EditIcon = Edit2;

  courseId = signal<string | null>(null);
  isEditMode = signal(false);

  readonly currentStep = signal(0);
  readonly saving = signal(false);
  readonly publishing = signal(false);
  readonly disciplines = signal<DisciplineDto[]>([]);

  readonly steps = [
    { label: 'Основное', icon: 'BookOpen' },
    { label: 'Структура', icon: 'FileText' },
    { label: 'Настройки', icon: 'Settings' },
    { label: 'Публикация', icon: 'Eye' },
  ];

  readonly levels = [
    { value: 'Beginner', label: 'Начальный' },
    { value: 'Intermediate', label: 'Средний' },
    { value: 'Advanced', label: 'Продвинутый' },
  ];

  // Step 1 form
  step1 = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(5)]],
    description: ['', [Validators.required, Validators.minLength(20)]],
    disciplineId: ['', Validators.required],
    level: ['Beginner', Validators.required],
    isFree: [true],
    price: [0],
  });

  // Step 2: modules state
  modules = signal<WizardModule[]>([]);

  // Step 3 form
  step3 = this.fb.group({
    maxAttempts: [3],
    passingScore: [70],
    deadline: [''],
    hasCertificate: [true],
    shuffleQuestions: [false],
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
    }
  }

  loadCourseForEdit(id: string): void {
    this.coursesService.getCourseById(id).subscribe({
      next: (course) => {
        this.step1.patchValue({
          title: course.title,
          description: course.description,
          disciplineId: (course as any).disciplineId ?? '',
          level: course.level,
          isFree: course.isFree,
          price: course.price ?? 0,
        });
        const modules: WizardModule[] = course.modules.map((m) => ({
          id: m.id,
          title: m.title,
          description: m.description ?? '',
          expanded: false,
          lessons: m.lessons.map((l) => ({
            id: l.id,
            title: l.title,
            description: l.description ?? '',
            duration: l.duration ?? null,
          })),
        }));
        this.modules.set(modules);
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

  // Navigation
  goToStep(step: number): void {
    if (step < this.currentStep()) {
      this.currentStep.set(step);
      return;
    }
    if (step === 1 && this.step1.invalid) {
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

  // Step 2: Module management
  addModule(): void {
    this.modules.update((mods) => [
      ...mods,
      { title: 'Новый модуль', description: '', lessons: [], expanded: true },
    ]);
  }

  removeModule(index: number): void {
    this.modules.update((mods) => mods.filter((_, i) => i !== index));
  }

  toggleModule(index: number): void {
    this.modules.update((mods) =>
      mods.map((m, i) => (i === index ? { ...m, expanded: !m.expanded } : m)),
    );
  }

  moveModule(index: number, dir: 'up' | 'down'): void {
    const mods = [...this.modules()];
    const target = dir === 'up' ? index - 1 : index + 1;
    if (target < 0 || target >= mods.length) return;
    [mods[index], mods[target]] = [mods[target], mods[index]];
    this.modules.set(mods);
  }

  updateModuleTitle(index: number, value: string): void {
    this.modules.update((mods) =>
      mods.map((m, i) => (i === index ? { ...m, title: value } : m)),
    );
  }

  updateModuleDesc(index: number, value: string): void {
    this.modules.update((mods) =>
      mods.map((m, i) => (i === index ? { ...m, description: value } : m)),
    );
  }

  addLesson(moduleIndex: number): void {
    this.modules.update((mods) =>
      mods.map((m, i) =>
        i === moduleIndex
          ? { ...m, lessons: [...m.lessons, { title: 'Новый урок', description: '', duration: null }] }
          : m,
      ),
    );
  }

  removeLesson(moduleIndex: number, lessonIndex: number): void {
    this.modules.update((mods) =>
      mods.map((m, i) =>
        i === moduleIndex
          ? { ...m, lessons: m.lessons.filter((_, li) => li !== lessonIndex) }
          : m,
      ),
    );
  }

  updateLessonTitle(moduleIndex: number, lessonIndex: number, value: string): void {
    this.modules.update((mods) =>
      mods.map((m, i) =>
        i === moduleIndex
          ? {
              ...m,
              lessons: m.lessons.map((l, li) =>
                li === lessonIndex ? { ...l, title: value } : l,
              ),
            }
          : m,
      ),
    );
  }

  moveLessonUp(moduleIndex: number, lessonIndex: number): void {
    if (lessonIndex === 0) return;
    this.modules.update((mods) =>
      mods.map((m, i) => {
        if (i !== moduleIndex) return m;
        const lessons = [...m.lessons];
        [lessons[lessonIndex - 1], lessons[lessonIndex]] = [lessons[lessonIndex], lessons[lessonIndex - 1]];
        return { ...m, lessons };
      }),
    );
  }

  moveLessonDown(moduleIndex: number, lessonIndex: number): void {
    this.modules.update((mods) =>
      mods.map((m, i) => {
        if (i !== moduleIndex) return m;
        if (lessonIndex >= m.lessons.length - 1) return m;
        const lessons = [...m.lessons];
        [lessons[lessonIndex], lessons[lessonIndex + 1]] = [lessons[lessonIndex + 1], lessons[lessonIndex]];
        return { ...m, lessons };
      }),
    );
  }

  // Step 4: Save/Publish
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
    const v3 = this.step3.value;

    const courseData: any = {
      title: v1.title,
      description: v1.description,
      disciplineId: v1.disciplineId,
      level: v1.level,
      isFree: v1.isFree,
      price: v1.isFree ? null : v1.price,
      orderType: v3.orderType,
      hasGrading: v3.hasGrading,
    };

    this.coursesService.createCourse(courseData).subscribe({
      next: (course) => {
        if (publish) {
          this.coursesService.publishCourse(course.id).subscribe({
            next: () => {
              this.saving.set(false);
              this.publishing.set(false);
              this.toastService.success('Курс опубликован!');
              this.router.navigate(['/teacher/courses']);
            },
            error: (err: ApiError) => {
              this.publishing.set(false);
              this.toastService.error(err.message);
            },
          });
        } else {
          this.saving.set(false);
          this.toastService.success('Черновик сохранён!');
          this.router.navigate(['/teacher/courses']);
        }
      },
      error: (err: ApiError) => {
        this.saving.set(false);
        this.publishing.set(false);
        this.toastService.error(err.message);
      },
    });
  }

  get selectedDisciplineName(): string {
    const id = this.step1.value.disciplineId;
    return this.disciplines().find((d) => d.id === id)?.name ?? '—';
  }

  get totalLessons(): number {
    return this.modules().reduce((sum, m) => sum + m.lessons.length, 0);
  }

  getFieldError(form: FormGroup, field: string): string {
    const ctrl = form.get(field);
    if (!ctrl?.touched || !ctrl.invalid) return '';
    if (ctrl.hasError('required')) return 'Обязательное поле';
    if (ctrl.hasError('minlength')) return `Минимум ${ctrl.errors?.['minlength']?.requiredLength} символов`;
    return 'Некорректное значение';
  }
}

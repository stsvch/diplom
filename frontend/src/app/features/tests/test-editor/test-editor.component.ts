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
import {
  LucideAngularModule,
  ChevronLeft,
  Plus,
  Trash2,
  ArrowUp,
  ArrowDown,
  Save,
  Loader2,
  ChevronDown,
  ChevronUp,
  AlignLeft,
  Settings,
  HelpCircle,
} from 'lucide-angular';
import { TestsService } from '../services/tests.service';
import { CoursesService } from '../../courses/services/courses.service';
import { CourseListDto } from '../../courses/models/course.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';
import { TestDetailDto, QuestionDto, AnswerOptionDto } from '../models/test.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';

interface LocalAnswerOption extends AnswerOptionDto {
  _localId: string;
}

interface LocalQuestion extends QuestionDto {
  answerOptions: LocalAnswerOption[];
  _collapsed?: boolean;
}

type QuestionType = 'SingleChoice' | 'MultipleChoice' | 'TextInput' | 'Matching' | 'OpenAnswer';

@Component({
  selector: 'app-test-editor',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    LucideAngularModule,
    ButtonComponent,
    BadgeComponent,
  ],
  templateUrl: './test-editor.component.html',
  styleUrl: './test-editor.component.scss',
})
export class TestEditorComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly testsService = inject(TestsService);
  private readonly coursesService = inject(CoursesService);
  private readonly toastService = inject(ToastService);

  readonly ChevronLeftIcon = ChevronLeft;
  readonly PlusIcon = Plus;
  readonly TrashIcon = Trash2;
  readonly ArrowUpIcon = ArrowUp;
  readonly ArrowDownIcon = ArrowDown;
  readonly SaveIcon = Save;
  readonly Loader2Icon = Loader2;
  readonly ChevronDownIcon = ChevronDown;
  readonly ChevronUpIcon = ChevronUp;
  readonly AlignLeftIcon = AlignLeft;
  readonly SettingsIcon = Settings;
  readonly HelpCircleIcon = HelpCircle;

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly showAddMenu = signal(false);
  readonly settingsCollapsed = signal(false);

  readonly testId = signal<string | null>(null);
  readonly isNew = signal(false);

  // Test settings form
  readonly title = signal('');
  readonly description = signal('');
  readonly courseId = signal<string>('');
  readonly courses = signal<CourseListDto[]>([]);
  readonly timeLimitMinutes = signal<number | null>(null);
  readonly maxAttempts = signal<number | null>(null);
  readonly deadline = signal('');
  readonly shuffleQuestions = signal(false);
  readonly shuffleAnswers = signal(false);
  readonly showCorrectAnswers = signal(false);

  readonly questions = signal<LocalQuestion[]>([]);

  private _localIdCounter = 0;

  get questionTypes(): QuestionType[] {
    return ['SingleChoice', 'MultipleChoice', 'TextInput', 'Matching', 'OpenAnswer'];
  }

  readonly questionTypeLabels: Record<QuestionType, string> = {
    SingleChoice: 'Один ответ',
    MultipleChoice: 'Несколько ответов',
    TextInput: 'Текстовый ответ',
    Matching: 'Соответствие',
    OpenAnswer: 'Открытый ответ',
  };

  ngOnInit(): void {
    this.coursesService.getMyCourses().subscribe({
      next: (list) => this.courses.set(list),
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.testId.set(id);
      this.isNew.set(false);
      this.loadTest(id);
    } else {
      this.isNew.set(true);
      this.loading.set(false);
    }
  }

  ngOnDestroy(): void {}

  private newLocalId(): string {
    return `local_${++this._localIdCounter}`;
  }

  loadTest(id: string): void {
    this.loading.set(true);
    this.testsService.getTest(id).subscribe({
      next: (data) => {
        this.title.set(data.title);
        this.description.set(data.description ?? '');
        this.courseId.set((data as any).courseId ?? '');
        this.timeLimitMinutes.set(data.timeLimitMinutes ?? null);
        this.maxAttempts.set(data.maxAttempts ?? null);
        this.deadline.set(data.deadline ? data.deadline.slice(0, 16) : '');
        this.shuffleQuestions.set(data.shuffleQuestions);
        this.shuffleAnswers.set(data.shuffleAnswers);
        this.showCorrectAnswers.set(data.showCorrectAnswers);

        const qs: LocalQuestion[] = (data.questions ?? [])
          .sort((a, b) => a.orderIndex - b.orderIndex)
          .map((q) => ({
            ...q,
            answerOptions: q.answerOptions.map((o) => ({
              ...o,
              _localId: this.newLocalId(),
            })),
          }));
        this.questions.set(qs);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  async saveTest(): Promise<void> {
    if (!this.title().trim()) {
      this.toastService.error('Введите название теста');
      return;
    }
    if (!this.courseId()) {
      this.toastService.error('Выберите курс');
      return;
    }
    this.saving.set(true);

    const payload = {
      courseId: this.courseId(),
      title: this.title().trim(),
      description: this.description() || undefined,
      timeLimitMinutes: this.timeLimitMinutes() ?? undefined,
      maxAttempts: this.maxAttempts() ?? undefined,
      deadline: this.deadline() ? new Date(this.deadline()).toISOString() : undefined,
      shuffleQuestions: this.shuffleQuestions(),
      shuffleAnswers: this.shuffleAnswers(),
      showCorrectAnswers: this.showCorrectAnswers(),
    };

    if (this.isNew()) {
      this.testsService.createTest(payload).subscribe({
        next: (test) => {
          this.testId.set(test.id);
          this.isNew.set(false);
          this.saving.set(false);
          this.toastService.success('Тест создан');
          this.router.navigate(['/teacher/test', test.id, 'edit']);
        },
        error: (err) => {
          this.saving.set(false);
          this.toastService.error(parseApiError(err).message);
        },
      });
    } else {
      this.testsService.updateTest(this.testId()!, payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.toastService.success('Тест сохранён');
        },
        error: (err) => {
          this.saving.set(false);
          this.toastService.error(parseApiError(err).message);
        },
      });
    }
  }

  addQuestion(type: QuestionType): void {
    this.showAddMenu.set(false);
    const testId = this.testId();
    if (!testId) {
      this.toastService.error('Сначала сохраните тест');
      return;
    }

    const defaultOptions = this.createDefaultOptions(type);

    this.testsService
      .createQuestion(testId, {
        type,
        text: '',
        points: 1,
        orderIndex: this.questions().length,
        answerOptions: defaultOptions,
      })
      .subscribe({
        next: (q) => {
          const localQ: LocalQuestion = {
            ...q,
            answerOptions: q.answerOptions.map((o) => ({
              ...o,
              _localId: this.newLocalId(),
            })),
          };
          this.questions.update((qs) => [...qs, localQ]);
          this.toastService.success('Вопрос добавлен');
        },
        error: (err) => {
          this.toastService.error(parseApiError(err).message);
        },
      });
  }

  private createDefaultOptions(type: QuestionType): AnswerOptionDto[] {
    const makeOpt = (overrides: Partial<AnswerOptionDto>): AnswerOptionDto => ({
      id: '',
      text: '',
      isCorrect: false,
      orderIndex: 0,
      ...overrides,
    });

    if (type === 'TextInput') {
      return [makeOpt({ isCorrect: true, orderIndex: 0 })];
    }
    if (type === 'SingleChoice' || type === 'MultipleChoice') {
      return [
        makeOpt({ orderIndex: 0 }),
        makeOpt({ orderIndex: 1 }),
      ];
    }
    if (type === 'Matching') {
      return [
        makeOpt({ isCorrect: true, orderIndex: 0, matchingPairValue: '' }),
        makeOpt({ isCorrect: true, orderIndex: 1, matchingPairValue: '' }),
      ];
    }
    return [];
  }

  deleteQuestion(questionId: string): void {
    this.testsService.deleteQuestion(questionId).subscribe({
      next: () => {
        this.questions.update((qs) => qs.filter((q) => q.id !== questionId));
        this.toastService.success('Вопрос удалён');
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  moveQuestion(index: number, direction: 'up' | 'down'): void {
    const qs = [...this.questions()];
    const target = direction === 'up' ? index - 1 : index + 1;
    if (target < 0 || target >= qs.length) return;
    [qs[index], qs[target]] = [qs[target], qs[index]];
    qs.forEach((q, i) => (q.orderIndex = i));
    this.questions.set(qs);

    const testId = this.testId();
    if (testId) {
      this.testsService.reorderQuestions(testId, qs.map((q) => q.id)).subscribe({
        error: (err) => this.toastService.error(parseApiError(err).message),
      });
    }
  }

  saveQuestion(q: LocalQuestion): void {
    const payload = {
      type: q.type,
      text: q.text,
      points: q.points,
      orderIndex: q.orderIndex,
      answerOptions: q.answerOptions.map((o, i) => ({
        id: o.id,
        text: o.text,
        isCorrect: o.isCorrect,
        orderIndex: i,
        matchingPairValue: o.matchingPairValue,
      })),
    };
    this.testsService.updateQuestion(q.id, payload).subscribe({
      next: (updated) => {
        this.questions.update((qs) =>
          qs.map((item) =>
            item.id === updated.id
              ? {
                  ...updated,
                  answerOptions: updated.answerOptions.map((o) => ({
                    ...o,
                    _localId: this.newLocalId(),
                  })),
                }
              : item,
          ),
        );
        this.toastService.success('Вопрос сохранён');
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  addOption(q: LocalQuestion): void {
    const newOpt: LocalAnswerOption = {
      id: '',
      text: '',
      isCorrect: false,
      orderIndex: q.answerOptions.length,
      _localId: this.newLocalId(),
    };
    this.updateQuestion(q.id, { answerOptions: [...q.answerOptions, newOpt] });
  }

  addMatchingOption(q: LocalQuestion): void {
    const newOpt: LocalAnswerOption = {
      id: '',
      text: '',
      isCorrect: true,
      orderIndex: q.answerOptions.length,
      matchingPairValue: '',
      _localId: this.newLocalId(),
    };
    this.updateQuestion(q.id, { answerOptions: [...q.answerOptions, newOpt] });
  }

  removeOption(q: LocalQuestion, localId: string): void {
    this.updateQuestion(q.id, {
      answerOptions: q.answerOptions.filter((o) => o._localId !== localId),
    });
  }

  setSingleCorrect(q: LocalQuestion, localId: string): void {
    this.updateQuestion(q.id, {
      answerOptions: q.answerOptions.map((o) => ({
        ...o,
        isCorrect: o._localId === localId,
      })),
    });
  }

  toggleMultiCorrect(q: LocalQuestion, localId: string): void {
    this.updateQuestion(q.id, {
      answerOptions: q.answerOptions.map((o) =>
        o._localId === localId ? { ...o, isCorrect: !o.isCorrect } : o,
      ),
    });
  }

  updateOptionText(q: LocalQuestion, localId: string, text: string): void {
    this.updateQuestion(q.id, {
      answerOptions: q.answerOptions.map((o) =>
        o._localId === localId ? { ...o, text } : o,
      ),
    });
  }

  updateMatchingPairValue(q: LocalQuestion, localId: string, value: string): void {
    this.updateQuestion(q.id, {
      answerOptions: q.answerOptions.map((o) =>
        o._localId === localId ? { ...o, matchingPairValue: value } : o,
      ),
    });
  }

  updateQuestionText(qId: string, text: string): void {
    this.updateQuestion(qId, { text });
  }

  updateQuestionPoints(qId: string, points: number): void {
    this.updateQuestion(qId, { points });
  }

  toggleQuestionCollapsed(qId: string): void {
    this.questions.update((qs) =>
      qs.map((q) => (q.id === qId ? { ...q, _collapsed: !q._collapsed } : q)),
    );
  }

  private updateQuestion(id: string, changes: Partial<LocalQuestion>): void {
    this.questions.update((qs) =>
      qs.map((q) => (q.id === id ? { ...q, ...changes } : q)),
    );
  }

  getQuestionTypeBadgeVariant(type: string): 'primary' | 'success' | 'warning' | 'danger' | 'neutral' {
    switch (type) {
      case 'SingleChoice': return 'primary';
      case 'MultipleChoice': return 'success';
      case 'TextInput': return 'warning';
      case 'Matching': return 'neutral';
      case 'OpenAnswer': return 'danger';
      default: return 'neutral';
    }
  }

  getQuestionTypeLabel(type: string): string {
    return this.questionTypeLabels[type as QuestionType] ?? type;
  }

  toggleAddMenu(): void {
    this.showAddMenu.update((v) => !v);
  }

  closeAddMenu(): void {
    this.showAddMenu.set(false);
  }

  toggleSettings(): void {
    this.settingsCollapsed.update((v) => !v);
  }

  get backUrl(): string {
    return '/teacher/courses';
  }
}

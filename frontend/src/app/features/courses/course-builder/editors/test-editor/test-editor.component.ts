import { Component, Input, OnDestroy, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EMPTY, Subject, catchError, concatMap, debounceTime, takeUntil, tap } from 'rxjs';
import {
  AlertCircle,
  Bot,
  CheckCircle2,
  CheckSquare,
  ChevronDown,
  ChevronUp,
  Circle,
  ClipboardCheck,
  Code as CodeIcon,
  GripVertical,
  Info,
  Lightbulb,
  ListChecks,
  LucideAngularModule,
  Plus,
  Settings,
  Square,
  Trash2,
  Type,
  User as UserIcon,
} from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';
import { TestsService } from '../../../../tests/services/tests.service';
import { ToastService } from '../../../../../shared/components/toast/toast.service';
import {
  AnswerOptionDto,
  QuestionDto,
  TestDetailDto,
} from '../../../../tests/models/test.model';

type SupportedQuestionType = 'SingleChoice' | 'MultipleChoice' | 'OpenAnswer' | 'Code';
type GradeKind = 'Auto' | 'Manual';

interface QuestionTypeOption {
  type: SupportedQuestionType;
  label: string;
  desc: string;
  icon: any;
  gradeType: GradeKind;
  /** CSS-modifier для цвета (orange / amber / blue / violet). */
  color: 'orange' | 'amber' | 'blue' | 'violet';
}

@Component({
  selector: 'app-cb-test-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './test-editor.component.html',
  styleUrl: './test-editor.component.scss',
})
export class TestEditorComponent implements OnDestroy {
  @Input({ required: true }) store!: CourseBuilderStore;

  private readonly api = inject(TestsService);
  private readonly toast = inject(ToastService);
  private readonly destroy$ = new Subject<void>();
  private readonly testSave$ = new Subject<Partial<TestDetailDto>>();
  private readonly questionSave$ = new Subject<QuestionDto>();

  readonly icons = {
    test: ClipboardCheck,
    settings: Settings,
    plus: Plus,
    trash: Trash2,
    alert: AlertCircle,
    check: CheckCircle2,
    type: Type,
    single: Circle,
    singleActive: CheckCircle2,
    multi: Square,
    multiActive: CheckSquare,
    listChecks: ListChecks,
    text: Type,
    code: CodeIcon,
    bulb: Lightbulb,
    info: Info,
    bot: Bot,
    user: UserIcon,
    grip: GripVertical,
    chevron: ChevronDown,
    chevronUp: ChevronUp,
  };

  readonly questionTypes: QuestionTypeOption[] = [
    { type: 'SingleChoice',   label: 'Один ответ',        desc: 'Студент выбирает один правильный ответ',           icon: Circle,     gradeType: 'Auto',   color: 'orange' },
    { type: 'MultipleChoice', label: 'Несколько ответов', desc: 'Студент выбирает один или несколько правильных',   icon: ListChecks, gradeType: 'Auto',   color: 'amber'  },
    { type: 'OpenAnswer',     label: 'Открытый ответ',    desc: 'Студент пишет развёрнутый ответ — проверяется вручную', icon: Type,  gradeType: 'Manual', color: 'blue'   },
    { type: 'Code',           label: 'Код',               desc: 'Студент пишет код — вручную или по эталонному выводу',  icon: CodeIcon, gradeType: 'Manual', color: 'violet' },
  ];

  readonly item = computed(() => this.store.selectedItem());
  readonly section = computed(() => this.store.selectedSection());

  readonly test = signal<TestDetailDto | null>(null);
  readonly loading = signal(false);
  readonly settingsOpen = signal(false);
  readonly typePickerOpen = signal(false);
  readonly collapsedQuestions = signal<ReadonlySet<string>>(new Set());
  /** id вопроса, у которого открыт popover смены типа. */
  readonly typeMenuFor = signal<string | null>(null);
  /** id вопроса, у которого раскрыто пояснение. */
  readonly explanationOpen = signal<ReadonlySet<string>>(new Set());

  readonly questions = computed(() => this.test()?.questions ?? []);
  readonly totalPoints = computed(() =>
    this.questions().reduce((sum, q) => sum + (q.points ?? 0), 0),
  );
  readonly autoCount = computed(() =>
    this.questions().filter((q) => this.gradeKindOf(q.type) === 'Auto').length,
  );
  readonly manualCount = computed(() =>
    this.questions().filter((q) => this.gradeKindOf(q.type) === 'Manual').length,
  );

  constructor() {
    effect(() => {
      const it = this.item();
      if (it && it.type === 'Test') {
        this.loadTest(it.sourceId);
      } else {
        this.test.set(null);
      }
    });

    this.testSave$
      .pipe(debounceTime(800), takeUntil(this.destroy$))
      .subscribe((patch) => this.persistTestSettings(patch));

    // concatMap гарантирует, что новый PUT вопроса не уходит, пока предыдущий не завершился —
    // иначе ловили DbUpdateConcurrencyException на параллельных правках.
    this.questionSave$
      .pipe(
        debounceTime(800),
        concatMap((q) => this.persistQuestion$(q)),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Загрузка ────────────────────────────────────────

  private loadTest(testId: string): void {
    this.loading.set(true);
    this.api.getTest(testId).subscribe({
      next: (t) => {
        this.test.set({ ...t, questions: [...t.questions].sort((a, b) => a.orderIndex - b.orderIndex) });
        this.loading.set(false);
        this.collapsedQuestions.set(new Set());
        this.explanationOpen.set(new Set());
      },
      error: () => {
        this.loading.set(false);
        this.toast.error('Не удалось загрузить тест');
      },
    });
  }

  // ── Заголовок ───────────────────────────────────────

  setTitle(value: string): void {
    const it = this.item();
    if (it) this.store.updateItemTitle(it, value);

    const t = this.test();
    if (t) {
      this.test.set({ ...t, title: value });
      this.testSave$.next({ title: value });
    }
  }

  // ── Настройки теста ─────────────────────────────────

  toggleSettings(): void {
    this.settingsOpen.update((v) => !v);
  }

  patchSettings(patch: Partial<TestDetailDto>): void {
    const t = this.test();
    if (!t) return;
    this.test.set({ ...t, ...patch });
    this.testSave$.next(patch);
  }

  setTimeLimit(value: string): void {
    const n = value === '' ? undefined : Number(value);
    if (n !== undefined && Number.isNaN(n)) return;
    this.patchSettings({ timeLimitMinutes: n });
  }

  setMaxAttempts(value: string): void {
    const n = value === '' ? undefined : Number(value);
    if (n !== undefined && Number.isNaN(n)) return;
    this.patchSettings({ maxAttempts: n });
  }

  setDeadline(value: string): void {
    this.patchSettings({ deadline: value || undefined });
  }

  toggleNoDeadline(noDeadline: boolean): void {
    if (noDeadline) {
      this.patchSettings({ deadline: undefined });
    } else {
      // Активируем поле — ставим дефолт +7 дней, чтобы datetime-local имело видимое значение.
      const d = new Date();
      d.setDate(d.getDate() + 7);
      d.setSeconds(0, 0);
      this.patchSettings({ deadline: d.toISOString() });
    }
  }

  setShuffleAnswers(v: boolean): void   { this.patchSettings({ shuffleAnswers: v }); }
  setShowCorrect(v: boolean): void      { this.patchSettings({ showCorrectAnswers: v }); }

  private persistTestSettings(patch: Partial<TestDetailDto>): void {
    const t = this.test();
    if (!t) return;
    const payload: Partial<TestDetailDto> = {
      courseId: t.courseId,
      title: t.title,
      description: t.description,
      timeLimitMinutes: t.timeLimitMinutes,
      maxAttempts: t.maxAttempts,
      deadline: t.deadline,
      shuffleQuestions: t.shuffleQuestions,
      shuffleAnswers: t.shuffleAnswers,
      showCorrectAnswers: t.showCorrectAnswers,
      ...patch,
    };
    this.api.updateTest(t.id, payload).subscribe({
      next: (saved) => {
        this.test.set({ ...saved, questions: t.questions });
      },
      error: (err) => {
        // eslint-disable-next-line no-console
        console.error('updateTest failed', err, payload);
        this.toast.error(this.formatApiError(err, 'Не удалось сохранить настройки теста'));
      },
    });
  }

  // ── Вопросы ─────────────────────────────────────────

  toggleTypePicker(): void {
    this.typePickerOpen.update((v) => !v);
  }

  toggleCollapse(qId: string): void {
    const next = new Set(this.collapsedQuestions());
    next.has(qId) ? next.delete(qId) : next.add(qId);
    this.collapsedQuestions.set(next);
  }

  isCollapsed(qId: string): boolean {
    return this.collapsedQuestions().has(qId);
  }

  toggleTypeMenu(qId: string): void {
    this.typeMenuFor.update((curr) => (curr === qId ? null : qId));
  }

  closeTypeMenu(): void {
    this.typeMenuFor.set(null);
  }

  toggleExplanation(qId: string): void {
    const next = new Set(this.explanationOpen());
    next.has(qId) ? next.delete(qId) : next.add(qId);
    this.explanationOpen.set(next);
  }

  isExplanationOpen(q: QuestionDto): boolean {
    return this.explanationOpen().has(q.id) || !!this.explanation(q);
  }

  addQuestion(type: SupportedQuestionType): void {
    const t = this.test();
    if (!t) return;
    this.typePickerOpen.set(false);

    const opt = this.questionTypes.find((o) => o.type === type)!;
    const payload: any = {
      type,
      text: '',
      points: 1,
      gradeType: opt.gradeType,
      answerOptions: this.defaultOptions(type),
    };

    this.api.createQuestion(t.id, payload).subscribe({
      next: (created) => {
        const next = [...t.questions, created].sort((a, b) => a.orderIndex - b.orderIndex);
        this.test.set({ ...t, questions: next });
        // Обновляем builder, чтобы readiness/счётчики в чеклисте курса
        // увидели свежий QuestionsCount.
        this.store.refresh();
      },
      error: (err) => {
        // eslint-disable-next-line no-console
        console.error('createQuestion failed', err, payload);
        this.toast.error(this.formatApiError(err, 'Не удалось создать вопрос'));
      },
    });
  }

  private defaultOptions(type: SupportedQuestionType): { text: string; isCorrect: boolean; matchingPairValue: string | null }[] {
    if (type === 'SingleChoice') {
      return [
        { text: '', isCorrect: false, matchingPairValue: null },
        { text: '', isCorrect: false, matchingPairValue: null },
      ];
    }
    if (type === 'MultipleChoice') {
      return [
        { text: '', isCorrect: false, matchingPairValue: null },
        { text: '', isCorrect: false, matchingPairValue: null },
      ];
    }
    return [];
  }

  removeQuestion(q: QuestionDto): void {
    const t = this.test();
    if (!t) return;
    this.api.deleteQuestion(q.id).subscribe({
      next: () => {
        this.test.set({ ...t, questions: t.questions.filter((x) => x.id !== q.id) });
        this.store.refresh();
      },
      error: () => this.toast.error('Не удалось удалить вопрос'),
    });
  }

  patchQuestion(q: QuestionDto, patch: Partial<QuestionDto>): void {
    const t = this.test();
    if (!t) return;
    const updated: QuestionDto = { ...q, ...patch };
    this.test.set({
      ...t,
      questions: t.questions.map((x) => (x.id === q.id ? updated : x)),
    });
    this.questionSave$.next(updated);
  }

  setPoints(q: QuestionDto, value: string): void {
    const n = Math.max(1, Number(value) || 1);
    this.patchQuestion(q, { points: n });
  }

  setQuestionType(q: QuestionDto, type: SupportedQuestionType): void {
    this.closeTypeMenu();
    const opt = this.questionTypes.find((o) => o.type === type)!;
    const newOptions = (type === 'SingleChoice' || type === 'MultipleChoice')
      ? (q.answerOptions.length > 0
          ? q.answerOptions.map((o) => ({ ...o, isCorrect: false }))
          : this.defaultOptions(type).map((d, i) => ({
              id: `tmp-${Date.now()}-${i}`,
              text: d.text,
              isCorrect: d.isCorrect,
              orderIndex: i,
            })))
      : [];

    this.patchQuestion(q, {
      type,
      answerOptions: newOptions,
    } as Partial<QuestionDto>);
    // gradeType вычисляется по типу в persistQuestion (gradeKindOf), отдельно хранить не нужно.
  }

  // ── Варианты ответа (для choice-вопросов) ───────────

  addOption(q: QuestionDto): void {
    const next: AnswerOptionDto = {
      id: `tmp-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
      text: '',
      isCorrect: false,
      orderIndex: q.answerOptions.length,
    };
    this.patchQuestion(q, { answerOptions: [...q.answerOptions, next] });
  }

  toggleOptionCorrect(q: QuestionDto, opt: AnswerOptionDto): void {
    const newCorrect = !opt.isCorrect;
    let nextOptions: AnswerOptionDto[];
    if (q.type === 'SingleChoice') {
      nextOptions = q.answerOptions.map((x) => ({ ...x, isCorrect: x.id === opt.id && newCorrect }));
    } else {
      nextOptions = q.answerOptions.map((x) => (x.id === opt.id ? { ...x, isCorrect: newCorrect } : x));
    }
    this.patchQuestion(q, { answerOptions: nextOptions });
  }

  setOptionText(q: QuestionDto, opt: AnswerOptionDto, text: string): void {
    const nextOptions = q.answerOptions.map((x) => (x.id === opt.id ? { ...x, text } : x));
    this.patchQuestion(q, { answerOptions: nextOptions });
  }

  removeOption(q: QuestionDto, opt: AnswerOptionDto): void {
    if (q.answerOptions.length <= 1) return;
    this.patchQuestion(q, { answerOptions: q.answerOptions.filter((x) => x.id !== opt.id) });
  }

  // ── Сохранение вопроса ─────────────────────────────

  private persistQuestion$(q: QuestionDto) {
    const payload: any = {
      type: q.type,
      text: q.text,
      points: q.points,
      gradeType: this.gradeKindOf(q.type),
      answerOptions: q.answerOptions.map((o) => ({
        id: this.isTempId(o.id) ? null : o.id,
        text: o.text,
        isCorrect: o.isCorrect,
        matchingPairValue: o.matchingPairValue ?? null,
      })),
      explanation: (q as any).explanation ?? null,
      expectedAnswer: (q as any).expectedAnswer ?? null,
    };

    return this.api.updateQuestion(q.id, payload).pipe(
      tap((saved) => {
        const t = this.test();
        if (!t) return;
        this.test.set({
          ...t,
          questions: t.questions.map((x) => (x.id === saved.id ? saved : x)),
        });
      }),
      catchError((err) => {
        // eslint-disable-next-line no-console
        console.error('updateQuestion failed', err, payload);
        this.toast.error(this.formatApiError(err, 'Не удалось сохранить вопрос'));
        // Возвращаем EMPTY чтобы concatMap не разорвал стрим — следующие save'ы продолжат идти.
        return EMPTY;
      }),
    );
  }

  /** Распаковываем ApiError: message + детали валидации/исключения. */
  private formatApiError(err: any, fallback: string): string {
    const apiError = err?.error;
    if (!apiError) return fallback;
    const baseMsg: string = apiError.message ?? fallback;
    const errors = apiError.errors as Record<string, string[]> | undefined;
    if (!errors) return baseMsg;
    const flat = Object.entries(errors)
      .filter(([, msgs]) => Array.isArray(msgs) && msgs.length > 0)
      .map(([field, msgs]) => `${field}: ${msgs.join(', ')}`)
      .join('; ');
    return flat ? `${baseMsg} — ${flat}` : baseMsg;
  }

  private gradeKindOf(type: string): GradeKind {
    return type === 'SingleChoice' || type === 'MultipleChoice' ? 'Auto' : 'Manual';
  }

  private isTempId(id: string): boolean {
    return id.startsWith('tmp-');
  }

  // ── Helpers для шаблона ─────────────────────────────

  configFor(type: string): QuestionTypeOption {
    return this.questionTypes.find((o) => o.type === type)
      ?? { type: 'OpenAnswer', label: type, desc: '', icon: Type, gradeType: 'Manual', color: 'blue' };
  }

  isChoiceType(type: string): boolean {
    return type === 'SingleChoice' || type === 'MultipleChoice';
  }

  isOpenAnswer(type: string): boolean {
    return type === 'OpenAnswer' || type === 'TextInput';
  }

  isCode(type: string): boolean {
    return type === 'Code';
  }

  hasAnyCorrect(q: QuestionDto): boolean {
    return q.answerOptions.some((o) => o.isCorrect);
  }

  formatDeadlineForInput(deadline?: string): string {
    if (!deadline) return '';
    const d = new Date(deadline);
    if (Number.isNaN(d.getTime())) return '';
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }

  // Casts для шаблона (fallback если бэк не возвращает поле)
  expectedAnswer(q: QuestionDto): string {
    return (q as any).expectedAnswer ?? '';
  }
  setExpectedAnswer(q: QuestionDto, value: string): void {
    this.patchQuestion(q, { ...(q as any), expectedAnswer: value } as Partial<QuestionDto>);
  }
  explanation(q: QuestionDto): string {
    return (q as any).explanation ?? '';
  }
  setExplanation(q: QuestionDto, value: string): void {
    this.patchQuestion(q, { ...(q as any), explanation: value } as Partial<QuestionDto>);
  }
}

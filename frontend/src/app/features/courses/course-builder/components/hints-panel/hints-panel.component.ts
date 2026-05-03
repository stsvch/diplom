import { Component, Input, Output, EventEmitter, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AlertCircle,
  CheckCircle2,
  ChevronRight,
  HelpCircle,
  Info,
  Layers,
  Lightbulb,
  LucideAngularModule,
  Play,
  RotateCcw,
  Sparkles,
  Target,
  XCircle,
  X,
  BookOpen,
} from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';
import { CourseItemType } from '../../models/course-builder.model';

interface ContextHint {
  icon: any;
  title: string;
  subtitle: string;
  steps: string[];
  readyWhen: string;
}

const HINTS: Record<CourseItemType, ContextHint> = {
  Lesson: {
    icon: BookOpen,
    title: 'Редактор урока',
    subtitle: 'Урок — страница в книге курса. Комбинируйте текст, видео, картинки и файлы.',
    steps: [
      'Назовите урок по теме (например: «Что такое UX»)',
      'Начните с текстового блока — вступление в тему',
      'Вставьте YouTube-ссылку — видео встроится автоматически',
      'Подкрепите изображением: схема, скриншот, иллюстрация',
      'Приложите PDF или файл как дополнительный материал',
    ],
    readyWhen: 'Есть хотя бы один заполненный блок контента',
  },
  Test: {
    icon: HelpCircle,
    title: 'Редактор теста',
    subtitle: 'Тест проверяет понимание материала. Делайте вопросы конкретными.',
    steps: [
      'Добавьте по одному вопросу на концепцию',
      'Заполните текст вопроса и варианты ответа',
      'Отметьте правильный ответ',
      'Настройте проходной балл (70–80%)',
      'Задайте количество попыток',
    ],
    readyWhen: 'Есть хотя бы 1 вопрос с правильным ответом',
  },
  Assignment: {
    icon: Sparkles,
    title: 'Редактор задания',
    subtitle: 'Задание — практическая работа. Ясное описание и критерии снимают 80% вопросов.',
    steps: [
      'Заполните описание задания — обязательно',
      'Объясните, что нужно сделать и что сдать',
      'Добавьте критерии оценивания',
      'Выберите формат: текст, файл или оба',
      'Установите дедлайн, если есть ограничение',
    ],
    readyWhen: 'Заполнено описание задания',
  },
  LiveSession: {
    icon: Play,
    title: 'Live-занятие',
    subtitle: 'Запланируйте онлайн-встречу. После заполнения она появится в расписании курса.',
    steps: [
      'Выберите дату и время занятия',
      'Вставьте ссылку на конференцию',
      'Укажите продолжительность',
      'Задайте лимит участников',
      'Выберите формат: групповое или индивидуальное',
    ],
    readyWhen: 'Заполнены дата, время и ссылка на встречу',
  },
  Resource: {
    icon: Info,
    title: 'Материал / Файл',
    subtitle: 'Дополнительный ресурс для скачивания: конспект, шаблон, презентация.',
    steps: [
      'Загрузите файл или вставьте прямую ссылку',
      'Дайте файлу понятное название',
    ],
    readyWhen: 'Указана ссылка на файл',
  },
  ExternalLink: {
    icon: Info,
    title: 'Внешняя ссылка',
    subtitle: 'Ссылка на ресурс вне платформы: статью, инструмент, библиотеку.',
    steps: [
      'Вставьте URL',
      'Добавьте понятную подпись — она станет кнопкой для студента',
    ],
    readyWhen: 'Указан URL',
  },
};

@Component({
  selector: 'app-cb-hints-panel',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './hints-panel.component.html',
  styleUrl: './hints-panel.component.scss',
})
export class HintsPanelComponent {
  @Input({ required: true }) store!: CourseBuilderStore;
  @Output() closeMobile = new EventEmitter<void>();

  readonly icons = {
    check: CheckCircle2,
    xcircle: XCircle,
    alert: AlertCircle,
    chev: ChevronRight,
    bulb: Lightbulb,
    help: HelpCircle,
    info: Info,
    sparkles: Sparkles,
    target: Target,
    layers: Layers,
    play: Play,
    rotate: RotateCcw,
    x: X,
    book: BookOpen,
  };

  readonly tab = signal<'checklist' | 'hints'>('checklist');

  readonly checklistItems = computed(() => {
    const r = this.store.readiness();
    if (!r) return [];
    return r.issues.map((iss, idx) => ({
      id: `iss-${idx}`,
      severity: iss.severity,
      label: iss.message,
      sectionId: iss.sectionId,
      sourceId: iss.sourceId,
    }));
  });

  readonly errorsCount = computed(() => this.store.readiness()?.errorCount ?? 0);
  readonly warningsCount = computed(() => this.store.readiness()?.warningCount ?? 0);

  readonly currentHint = computed<ContextHint | null>(() => {
    const item = this.store.selectedItem();
    if (!item) return null;
    return HINTS[item.type] ?? null;
  });

  /** Длина окружности для прогресс-кольца (r=18, 2πr ≈ 113.1) */
  readonly ringCircumference = 113.1;
  readonly ringOffset = computed(() => {
    const p = this.store.progressPercent();
    return this.ringCircumference * (1 - p / 100);
  });

  goIssue(issue: { sectionId?: string | null; sourceId?: string | null }): void {
    if (issue.sourceId) {
      this.store.setSelection({
        kind: 'item',
        sectionId: issue.sectionId ?? null,
        itemId: issue.sourceId,
      });
    }
  }

  reopenOnboarding(): void {
    this.store.showOnboarding.set(true);
  }
}

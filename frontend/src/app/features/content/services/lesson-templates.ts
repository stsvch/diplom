import {
  LessonBlockType,
  LessonBlockData,
  LessonBlockSettings,
  defaultBlockData,
  defaultSettings,
} from '../models';

export interface TemplateBlock {
  type: LessonBlockType;
  data: LessonBlockData;
  settings: LessonBlockSettings;
}

export interface LessonTemplate {
  id: string;
  name: string;
  description: string;
  blocks: TemplateBlock[];
}

function mk(type: LessonBlockType, dataPatch: Partial<LessonBlockData> = {}, required = true): TemplateBlock {
  const data = { ...defaultBlockData(type), ...(dataPatch as object) } as LessonBlockData;
  const settings = { ...defaultSettings(), requiredForCompletion: required };
  return { type, data, settings };
}

export const LESSON_TEMPLATES: LessonTemplate[] = [
  {
    id: 'empty',
    name: 'Пустой урок',
    description: 'Начните с чистого листа',
    blocks: [],
  },
  {
    id: 'lecture',
    name: 'Лекция',
    description: 'Баннер с темой, теория, видео',
    blocks: [
      mk('Banner', { title: 'Новая тема' } as any, false),
      mk('Text', { html: '<p>Введение в тему...</p>' } as any, false),
      mk('Video', {} as any, false),
      mk('Text', { html: '<h3>Итог</h3><p>Основные моменты:</p>' } as any, false),
    ],
  },
  {
    id: 'practice',
    name: 'Практика',
    description: 'Текст + несколько упражнений',
    blocks: [
      mk('Text', { html: '<p>Прочитайте материал и выполните упражнения.</p>' } as any, false),
      mk('SingleChoice'),
      mk('FillGap'),
      mk('TrueFalse'),
    ],
  },
  {
    id: 'quiz-lesson',
    name: 'Тестовый урок',
    description: 'Один встроенный тест',
    blocks: [
      mk('Text', { html: '<p>Проверьте свои знания по теме.</p>' } as any, false),
      mk('Quiz'),
    ],
  },
];

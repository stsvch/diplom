import { LessonBlockType } from './block-type.model';
import {
  LessonBlockData,
  LessonBlockSettings,
} from './block-data.model';

export const DEFAULT_SETTINGS: LessonBlockSettings = {
  points: 1,
  requiredForCompletion: true,
  showFeedback: true,
};

export function defaultBlockData(type: LessonBlockType): LessonBlockData {
  switch (type) {
    case 'Text':
      return { type: 'Text', html: '' };
    case 'Video':
      return { type: 'Video', url: '' };
    case 'Audio':
      return { type: 'Audio', url: '' };
    case 'Image':
      return { type: 'Image', url: '' };
    case 'Banner':
      return { type: 'Banner', title: 'Заголовок' };
    case 'File':
      return { type: 'File', attachmentId: '' };

    case 'SingleChoice':
      return {
        type: 'SingleChoice',
        question: '',
        options: [
          { id: 'a', text: '', isCorrect: true },
          { id: 'b', text: '', isCorrect: false },
        ],
      };
    case 'MultipleChoice':
      return {
        type: 'MultipleChoice',
        question: '',
        partialCredit: true,
        options: [
          { id: 'a', text: '', isCorrect: false },
          { id: 'b', text: '', isCorrect: false },
        ],
      };
    case 'TrueFalse':
      return {
        type: 'TrueFalse',
        statements: [{ id: 's1', text: '', isTrue: true }],
      };
    case 'FillGap':
      return {
        type: 'FillGap',
        sentences: [
          {
            id: 's1',
            template: '',
            gaps: [{ id: '0', correctAnswers: [''], caseSensitive: false }],
          },
        ],
      };
    case 'Dropdown':
      return {
        type: 'Dropdown',
        sentences: [
          {
            id: 's1',
            template: '',
            gaps: [{ id: '0', options: ['', ''], correct: '' }],
          },
        ],
      };
    case 'WordBank':
      return {
        type: 'WordBank',
        bank: [],
        allowExtraWords: true,
        sentences: [{ id: 's1', template: '', correctAnswers: [] }],
      };
    case 'Reorder':
      return {
        type: 'Reorder',
        allOrNothing: true,
        items: [
          { id: 'i1', text: '' },
          { id: 'i2', text: '' },
        ],
        correctOrder: ['i1', 'i2'],
      };
    case 'Matching':
      return {
        type: 'Matching',
        leftItems: [{ id: 'l1', text: '' }],
        rightItems: [{ id: 'r1', text: '' }],
        correctPairs: [{ leftId: 'l1', rightId: 'r1' }],
      };

    case 'OpenText':
      return {
        type: 'OpenText',
        instruction: '',
        helperWords: [],
        unit: 'Chars',
      };
    case 'CodeExercise':
      return {
        type: 'CodeExercise',
        instruction: '',
        language: 'javascript',
        testCases: [],
        timeoutMs: 5000,
        memoryLimitMb: 128,
        hiddenTests: false,
      };

    case 'Quiz':
      return { type: 'Quiz', testId: '' };
    case 'Assignment':
      return { type: 'Assignment', assignmentId: '' };
  }
}

export function defaultSettings(): LessonBlockSettings {
  return { ...DEFAULT_SETTINGS };
}

export const BLOCK_TYPE_LABELS: Record<LessonBlockType, string> = {
  Text: 'Текст',
  Video: 'Видео',
  Audio: 'Аудио',
  Image: 'Изображение',
  Banner: 'Баннер',
  File: 'Файл',
  SingleChoice: 'Один вариант',
  MultipleChoice: 'Несколько вариантов',
  TrueFalse: 'Верно / Неверно',
  FillGap: 'Пропуски (ввод)',
  Dropdown: 'Пропуски (выбор)',
  WordBank: 'Банк слов',
  Reorder: 'Порядок карточек',
  Matching: 'Сопоставление',
  OpenText: 'Открытый ответ',
  CodeExercise: 'Упражнение по коду',
  Quiz: 'Встроенный тест',
  Assignment: 'Встроенное задание',
};

export const BLOCK_TYPE_ICONS: Record<LessonBlockType, string> = {
  Text: 'type',
  Video: 'video',
  Audio: 'music',
  Image: 'image',
  Banner: 'flag',
  File: 'paperclip',
  SingleChoice: 'circle-dot',
  MultipleChoice: 'check-square',
  TrueFalse: 'toggle-left',
  FillGap: 'text-cursor-input',
  Dropdown: 'chevron-down-square',
  WordBank: 'package-plus',
  Reorder: 'list-ordered',
  Matching: 'arrow-left-right',
  OpenText: 'file-text',
  CodeExercise: 'code',
  Quiz: 'clipboard-list',
  Assignment: 'briefcase',
};

export interface TemplateLesson {
  title: string;
  description?: string;
}

export interface TemplateModule {
  title: string;
  description?: string;
  lessons: TemplateLesson[];
}

export interface CourseTemplate {
  id: string;
  name: string;
  description: string;
  icon: string;
  modules: TemplateModule[];
}

export const COURSE_TEMPLATES: CourseTemplate[] = [
  {
    id: 'empty',
    name: 'Пустой курс',
    description: 'Начните с чистого листа и добавьте модули сами',
    icon: 'file-plus',
    modules: [],
  },
  {
    id: 'lecture',
    name: 'Лекционный курс',
    description: '3 модуля по 3 лекции — для теоретических курсов',
    icon: 'graduation-cap',
    modules: [
      {
        title: 'Введение',
        lessons: [
          { title: 'Знакомство с темой' },
          { title: 'История и основные понятия' },
          { title: 'Практическая значимость' },
        ],
      },
      {
        title: 'Основы',
        lessons: [
          { title: 'Базовые принципы' },
          { title: 'Ключевые концепции' },
          { title: 'Примеры применения' },
        ],
      },
      {
        title: 'Заключение',
        lessons: [
          { title: 'Обобщение материала' },
          { title: 'Связь с другими темами' },
          { title: 'Итоговая рефлексия' },
        ],
      },
    ],
  },
  {
    id: 'intensive',
    name: 'Интенсив',
    description: '10 уроков подряд без деления на модули',
    icon: 'zap',
    modules: [
      {
        title: 'Полный курс',
        lessons: Array.from({ length: 10 }, (_, i) => ({ title: `Урок ${i + 1}` })),
      },
    ],
  },
  {
    id: 'quiz-course',
    name: 'Курс с тестами',
    description: '5 модулей, каждый завершается контрольным уроком',
    icon: 'clipboard-check',
    modules: Array.from({ length: 5 }, (_, i) => ({
      title: `Тема ${i + 1}`,
      lessons: [
        { title: 'Лекция' },
        { title: 'Разбор примеров' },
        { title: `Тест по теме ${i + 1}` },
      ],
    })),
  },
];

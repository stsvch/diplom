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
  icon: 'mini' | 'tests' | 'homework';
  badges: string[];
  modules: TemplateModule[];
}

export const COURSE_TEMPLATES: CourseTemplate[] = [
  {
    id: 'mini',
    name: 'Мини-курс',
    description: '2 раздела, по 2 урока в каждом. Идеально для короткого курса или мастер-класса.',
    icon: 'mini',
    badges: ['Урок', 'Урок'],
    modules: [
      {
        title: 'Раздел 1',
        lessons: [{ title: 'Урок 1' }, { title: 'Урок 2' }],
      },
      {
        title: 'Раздел 2',
        lessons: [{ title: 'Урок 1' }, { title: 'Урок 2' }],
      },
    ],
  },
  {
    id: 'with-tests',
    name: 'Курс с тестами',
    description: '3 раздела: урок → тест в каждом. Для курсов с проверкой знаний.',
    icon: 'tests',
    badges: ['Урок', 'Тест'],
    modules: [
      { title: 'Раздел 1', lessons: [{ title: 'Урок' }, { title: 'Контрольный урок' }] },
      { title: 'Раздел 2', lessons: [{ title: 'Урок' }, { title: 'Контрольный урок' }] },
      { title: 'Раздел 3', lessons: [{ title: 'Урок' }, { title: 'Контрольный урок' }] },
    ],
  },
  {
    id: 'with-homework',
    name: 'Курс с заданиями',
    description: '3 раздела: урок → задание. Для практико-ориентированных курсов.',
    icon: 'homework',
    badges: ['Урок', 'Задание'],
    modules: [
      { title: 'Раздел 1', lessons: [{ title: 'Урок' }, { title: 'Практическая работа' }] },
      { title: 'Раздел 2', lessons: [{ title: 'Урок' }, { title: 'Практическая работа' }] },
      { title: 'Раздел 3', lessons: [{ title: 'Урок' }, { title: 'Практическая работа' }] },
    ],
  },
];

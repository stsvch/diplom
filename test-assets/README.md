# test-assets — материалы для наполнения платформы

Папка с тестовыми данными для ручного наполнения EduPlatform курсами, уроками,
заданиями и тестами. Файлы организованы по типу контента и по темам.

## Структура

```
test-assets/
├── images/
│   ├── course-covers/         — 9 SVG-обложек курсов (1200×630, подходят для карточек)
│   ├── avatars/               — 9 фото-аватаров (student-1..5, teacher-1..3, admin-1)
│   └── lesson-photos/         — 8 фото для иллюстраций к урокам (1200×800)
│
├── videos/
│   ├── programming-videos.md  — ссылки на YouTube: Python, JS, веб, Git
│   ├── english-videos.md      — ссылки: грамматика, Business English, произношение
│   └── math-and-design-videos.md — математика, дизайн, маркетинг
│
├── audio/
│   ├── english-pronunciation.md — источники произношения (Forvo, Cambridge, TTS)
│   └── free-audio-sources.md    — Freesound, BBC, подкасты, FMA
│
├── documents/
│   ├── python-lesson-1.md     — пример текстового урока по Python
│   ├── javascript-lesson-1.md — пример урока по JS
│   ├── english-lesson-1.md    — пример урока по английскому
│   └── math-derivative-intro.md — пример урока по математике
│
└── data/
    ├── courses-seed.json      — список дисциплин и курсов для быстрого наполнения
    ├── dictionary-english.json — 20 слов для модуля Tools (словарь)
    ├── code-exercises.json    — 7 упражнений по Python для модуля Tools (редактор кода)
    ├── test-questions.json    — примеры тестов по 3 дисциплинам
    └── assignments-seed.json  — 5 заданий для разных курсов
```

## Что куда грузить в платформу

| Где в UI | Что использовать |
|---|---|
| Создание курса → обложка | `images/course-covers/*.svg` |
| Редактор урока → блок «Изображение» | `images/lesson-photos/*.jpg` |
| Редактор урока → блок «Видео URL» | ссылки из `videos/*.md` |
| Редактор урока → блок «Аудио URL» | ссылки из `audio/*.md` |
| Редактор урока → блок «Текст» / «Markdown» | содержимое `documents/*.md` |
| Профиль пользователя → аватар | `images/avatars/*.jpg` |
| Модуль Tools → Словарь | импорт `data/dictionary-english.json` |
| Модуль Tools → Упражнения по коду | импорт `data/code-exercises.json` |
| Создание теста | вопросы из `data/test-questions.json` |
| Создание задания | шаблоны из `data/assignments-seed.json` |

## Темы, охваченные в материалах

- **Программирование** — Python, JavaScript, HTML/CSS, Git
- **Иностранные языки** — English (A1–A2, Business)
- **Математика** — алгебра, геометрия, мат. анализ
- **Дизайн** — UI/UX, Figma, типографика, цвет
- **Бизнес / маркетинг** — SMM, SEO, digital-marketing

## Лицензии

- **Фото** (`images/lesson-photos/`, `images/avatars/`) — скачаны с [picsum.photos](https://picsum.photos),
  под лицензией [Unsplash License](https://unsplash.com/license) (бесплатно для любого
  использования, в т.ч. коммерческого, без указания авторства).
- **SVG-обложки** — сгенерированы вручную, можно свободно редактировать.
- **Видео/аудио по ссылкам** — публично доступны на YouTube / BBC / TED и других
  платформах. Для реального использования в коммерческом продукте проверьте условия
  каждого источника.
- **Тексты уроков, тестов, упражнений** — написаны для учебных целей диплома, свободны
  к использованию и редактированию.

## Как добавить своё

- Новые обложки — кладите SVG/JPG/PNG в `images/course-covers/`.
- Новые уроки — `.md` в `documents/`.
- Новые слова в словарь — расширяйте массив `words` в `dictionary-english.json`.

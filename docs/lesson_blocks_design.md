# Дизайн системы блоков урока

Документ фиксирует полный набор типов учебных блоков, структуру данных каждого типа, API, модель попыток студента, UX редактора и прохождения урока, а также этапы реализации.

Дата: 2026-04-19
Автор: stsvch

---

## 1. Концепция

Урок состоит из упорядоченной последовательности **блоков**. Блок — это единица контента со своим типом поведения: информационный, интерактивный с авто-проверкой, или составной (ссылается на Test/Assignment).

**Модуль Content** отвечает за:
- Хранение блоков (таблица `content.lesson_blocks`)
- Хранение попыток студента (таблица `content.lesson_block_attempts`)
- Автопроверку интерактивных блоков
- Вложения (`content.attachments`) для блоков и иных сущностей

**Модуль Courses** отвечает только за:
- Структуру курса (Course → Module → Lesson)
- Настройки урока (title, orderIndex, layout)

**Модули Tests / Assignments** — отдельные сущности, которые могут быть встроены в урок через блоки типа `Quiz` / `Assignment` (блок хранит только id).

---

## 2. Типы блоков (полный список)

### 2.1. Информационные блоки (без проверки)

| Тип | Назначение |
|-----|------------|
| `Text` | Rich-text фрагмент (WYSIWYG: заголовки, списки, таблицы, цитаты, код) |
| `Video` | Видео по URL или загруженное в MinIO |
| `Audio` | Аудиозапись с плеером (подкасты, аудирование) |
| `Image` | Одно изображение |
| `Banner` | Декоративная плашка: большой заголовок + картинка + цвет фона |
| `File` | Прикреплённый файл для скачивания |

### 2.2. Интерактивные блоки (авто-проверка)

| Тип | Назначение |
|-----|------------|
| `SingleChoice` | Один правильный ответ из N вариантов |
| `MultipleChoice` | Несколько правильных ответов из N |
| `TrueFalse` | Верно/Неверно |
| `FillGap` | Пропуски в тексте — студент пишет ответ сам |
| `Dropdown` | Пропуски в тексте — студент выбирает из выпадающего списка |
| `WordBank` | Банк слов сверху, пропуски в предложениях — перетащить слова |
| `Reorder` | Расставить карточки в правильном порядке |
| `Matching` | Соединить пары (термин — определение, вопрос — ответ) |

### 2.3. Интерактивные блоки (ручная/полуавтоматическая проверка)

| Тип | Назначение |
|-----|------------|
| `OpenText` | Свободный текстовый ответ (эссе, writing, короткий ответ). Проверяется преподавателем |
| `CodeExercise` | Редактор кода (Monaco), автозапуск тестов, проверка результата выполнения |

### 2.4. Составные блоки (ссылки на другие модули)

| Тип | Назначение |
|-----|------------|
| `Quiz` | Встроенный тест из модуля Tests (большой, с таймером, попытками) |
| `Assignment` | Встроенное задание из модуля Assignments (с дедлайном, файлами, проверкой) |

**Итого:** 18 типов блоков.

---

## 3. Модель данных

### 3.1. Таблица `content.lesson_blocks`

| Поле | Тип | Описание |
|------|-----|----------|
| `id` | uuid PK | |
| `lesson_id` | uuid FK → `courses.lessons(id)` ON DELETE CASCADE | Cross-schema FK |
| `order_index` | int | Порядок внутри урока (0-based) |
| `type` | int (enum `LessonBlockType`) | |
| `data` | jsonb | Контент блока, структура зависит от `type` |
| `settings` | jsonb | Общие настройки блока (см. 3.3) |
| `created_at` | timestamp | |
| `updated_at` | timestamp NULL | |

Индексы: `(lesson_id, order_index)`, `type`.

### 3.2. Таблица `content.lesson_block_attempts`

| Поле | Тип | Описание |
|------|-----|----------|
| `id` | uuid PK | |
| `block_id` | uuid FK → `lesson_blocks(id)` ON DELETE CASCADE | |
| `user_id` | uuid | |
| `answers` | jsonb | Ответы студента, структура зависит от `type` блока |
| `score` | numeric(5,2) | Набранные баллы |
| `max_score` | numeric(5,2) | Максимум для этого блока |
| `is_correct` | bool | true, если score == max_score |
| `needs_review` | bool | true для OpenText/CodeExercise до ручной проверки |
| `status` | int (enum: Draft, Submitted, Graded) | |
| `submitted_at` | timestamp | |
| `reviewed_at` | timestamp NULL | |
| `reviewer_id` | uuid NULL | |
| `reviewer_comment` | text NULL | |

Уникальный индекс: `(block_id, user_id)` — одна попытка на пользователя на блок (при повторной отправке — апсёрт).

### 3.3. Структура `settings` (общая для всех типов)

```json
{
  "points": 1.0,                    // баллы за успешное прохождение
  "requiredForCompletion": true,    // влияет ли на автозавершение урока
  "hint": "Обратите внимание на...", // подсказка, показывается по кнопке
  "shuffleOptions": false           // для Choice/Reorder — перемешивать ли варианты
}
```

---

## 4. Структура `data` по каждому типу

Поля, помеченные `*`, — обязательные.

### 4.1. `Text`
```json
{ "html": "<p>...</p>"* }
```

### 4.2. `Video`
```json
{
  "url": "https://..."*,
  "caption": "Подпись",
  "posterUrl": "https://..."
}
```

### 4.3. `Audio`
```json
{
  "url": "https://..."*,
  "transcript": "Текст расшифровки",
  "duration": 174
}
```

### 4.4. `Image`
```json
{
  "url": "https://..."*,
  "alt": "Описание",
  "caption": "Подпись"
}
```

### 4.5. `Banner`
```json
{
  "title": "Let's meditate"*,
  "bgColor": "#b0e86a",
  "textColor": "#ffffff",
  "imageUrl": "https://..."
}
```

### 4.6. `File`
```json
{
  "attachmentId": "uuid"*,
  "displayName": "lesson-notes.pdf",
  "description": "Конспект урока"
}
```

### 4.7. `SingleChoice`
```json
{
  "instruction": "Выберите правильный ответ"*,
  "question": "Сколько шагов должен делать человек в день?"*,
  "imageUrl": "https://...",
  "options": [
    { "id": "a"*, "text": "10 000"*, "isCorrect": true* },
    { "id": "b",  "text": "1 000",   "isCorrect": false }
  ]
}
```

### 4.8. `MultipleChoice`
Как `SingleChoice`, но `isCorrect: true` может быть у нескольких опций. Правильным считается полное совпадение выбранных.

### 4.9. `TrueFalse`
```json
{
  "instruction": "Верно или неверно",
  "statements": [
    { "id": "s1"*, "text": "Вода кипит при 100°C"*, "isTrue": true* }
  ]
}
```

### 4.10. `FillGap`
```json
{
  "instruction": "Вставьте пропущенные слова",
  "sentences": [
    {
      "id": "s1"*,
      "template": "If you {{0}} a healthy diet, you {{1}} fit."*,
      "gaps": [
        { "id": "0"*, "correctAnswers": ["eat","keep"]*, "caseSensitive": false },
        { "id": "1", "correctAnswers": ["will be"],      "caseSensitive": false }
      ]
    }
  ]
}
```
Шаблон — текст с плейсхолдерами `{{0}}`, `{{1}}` и т.д., их количество равно длине `gaps`.

### 4.11. `Dropdown`
```json
{
  "instruction": "Выберите правильные варианты",
  "sentences": [
    {
      "id": "s1"*,
      "template": "If you {{0}} a healthy diet, you will be fit."*,
      "gaps": [
        { "id": "0"*, "options": ["eat","will eat","would eat"]*, "correct": "eat"* }
      ]
    }
  ]
}
```

### 4.12. `WordBank`
```json
{
  "instruction": "Заполните пропуски словами из банка",
  "bank": ["hydrated","released","deal with","assessment","aid"]*,
  "sentences": [
    {
      "id": "s1",
      "template": "Meditation apps help to {{0}} stress and {{1}} effectively."*,
      "correctAnswers": ["deal with","aid"]*
    }
  ],
  "allowExtraWords": true
}
```

### 4.13. `Reorder`
```json
{
  "instruction": "Расставьте в правильном порядке",
  "items": [
    { "id": "a"*, "text": "Let your mind focus on your breath."* },
    { "id": "b",  "text": "Open your eyes and stretch." }
  ],
  "correctOrder": ["b","a","c"]*
}
```

### 4.14. `Matching`
```json
{
  "instruction": "Соедините пары",
  "leftItems":  [ { "id": "l1"*, "text": "Fitness app"* } ],
  "rightItems": [ { "id": "r1"*, "text": "Counts steps"* } ],
  "correctPairs": [ { "leftId": "l1"*, "rightId": "r1"* } ]
}
```

### 4.15. `OpenText`
```json
{
  "instruction": "Напишите промо для вашего приложения",*
  "prompt": "Используя слова ниже, напишите текст...",
  "helperWords": ["anxiety","app","wellness"],
  "minLength": 50,
  "maxLength": 900,
  "unit": "chars"
}
```
`unit`: `"chars"` или `"words"`.

### 4.16. `CodeExercise`
```json
{
  "instruction": "Напишите функцию, возвращающую сумму",*
  "language": "csharp"*,
  "starterCode": "public int Sum(int a, int b) { return 0; }",
  "testCases": [
    { "input": "2 3",  "expectedOutput": "5" },
    { "input": "0 0",  "expectedOutput": "0" }
  ],
  "timeoutMs": 5000,
  "memoryLimitMb": 128
}
```

### 4.17. `Quiz`
```json
{ "testId": "uuid"* }
```
Метаданные теста (название, число вопросов) подтягиваются фронтом через `GET /api/tests/{testId}`.

### 4.18. `Assignment`
```json
{ "assignmentId": "uuid"* }
```

---

## 5. Структура `answers` в попытке (на каждый тип)

Отражает формат ввода студента.

| Тип | `answers` |
|-----|-----------|
| `SingleChoice` | `{ "selectedOptionId": "a" }` |
| `MultipleChoice` | `{ "selectedOptionIds": ["a","c"] }` |
| `TrueFalse` | `{ "responses": [ { "statementId": "s1", "answer": true } ] }` |
| `FillGap` | `{ "responses": [ { "sentenceId": "s1", "gaps": [ { "gapId": "0", "value": "eat" } ] } ] }` |
| `Dropdown` | как FillGap |
| `WordBank` | `{ "responses": [ { "sentenceId": "s1", "answers": ["deal with","aid"] } ] }` |
| `Reorder` | `{ "order": ["b","a","c"] }` |
| `Matching` | `{ "pairs": [ { "leftId": "l1", "rightId": "r1" } ] }` |
| `OpenText` | `{ "text": "..." }` |
| `CodeExercise` | `{ "code": "..." , "runOutput": [ ... ] }` |

Информационные блоки (`Text`, `Video`, `Audio`, `Image`, `Banner`, `File`) и составные (`Quiz`, `Assignment`) **не создают попыток** в `lesson_block_attempts`. Для Quiz/Assignment прогресс берётся из модулей Tests/Assignments.

---

## 6. Правила автопроверки

| Тип | Алгоритм | Частичные баллы |
|-----|----------|-----------------|
| `SingleChoice` | `selectedOptionId == correctOption.id` | Нет — либо 100%, либо 0% |
| `MultipleChoice` | Множество выбранных == множество правильных | Да (доля совпавших) — настраивается |
| `TrueFalse` | Каждый statement — 1/N | Да |
| `FillGap` | Каждый gap — 1/N. Trim, нормализация регистра, сравнение с `correctAnswers` массивом | Да |
| `Dropdown` | Каждый gap — 1/N. Точное совпадение с `correct` | Да |
| `WordBank` | Каждый gap — 1/N. Точное совпадение | Да |
| `Reorder` | Порядок совпадает с `correctOrder` | Либо всё, либо ничего (настраивается) |
| `Matching` | Каждая пара — 1/N | Да |
| `OpenText` | `needsReview = true`, score=0 до проверки | — |
| `CodeExercise` | Запуск testCases, доля пройденных | Да |

---

## 7. Публичный API

### 7.1. Работа с блоками (преподаватель)

```
GET    /api/lesson-blocks/by-lesson/{lessonId}     — получить блоки урока
POST   /api/lesson-blocks                           — создать блок
PUT    /api/lesson-blocks/{id}                      — обновить блок
DELETE /api/lesson-blocks/{id}                      — удалить блок
POST   /api/lesson-blocks/reorder                   — изменить порядок
```

### 7.2. Прохождение (студент)

```
POST   /api/lesson-blocks/{id}/attempts             — отправить ответ
       body: { answers: <тип-зависимое> }
       response: { score, maxScore, isCorrect, needsReview, feedback? }

GET    /api/lessons/{lessonId}/my-progress          — сводка по уроку
       response: {
         lessonId, totalBlocks, requiredBlocks, completedBlocks,
         totalScore, maxScore, percentage, isCompleted
       }

GET    /api/lesson-blocks/{id}/my-attempt           — моя текущая попытка
```

### 7.3. Проверка открытых ответов (преподаватель)

```
GET    /api/lessons/{lessonId}/attempts?userId={id} — попытки студента по уроку
POST   /api/lesson-block-attempts/{id}/review       — выставить оценку
       body: { score, comment }
```

---

## 8. Взаимодействие модулей

```
Courses (структура)        Content (контент + попытки)        Tests / Assignments
───────────────────        ──────────────────────────         ───────────────────
Course                     LessonBlock (data jsonb)           Test
  ModuleSet                  Attachment                         Question/Attempt
    Lesson                   LessonBlockAttempt                 (независимо)
    (layout: Scroll/Stepper)
                                                              Assignment
                                                                Submission
```

**Контракты в Shared** (для изоляции модулей):

1. **`ILessonContentCleaner`** — Content реализует, Courses вызывает при удалении урока.
   ```csharp
   Task DeleteByLessonIdAsync(Guid lessonId, CancellationToken ct);
   ```

2. **`ICourseContentCleaner`** — каскадная очистка контента всего курса (при удалении курса).
   ```csharp
   Task DeleteByCourseIdAsync(Guid courseId, IEnumerable<Guid> lessonIds, CancellationToken ct);
   ```

3. **`IContentReadService`** (опционально, для Courses/Progress) — чтение сводной инфы о блоках.
   ```csharp
   Task<int> GetBlocksCountAsync(Guid lessonId, CancellationToken ct);
   Task<LessonCompletionInfo> GetCompletionAsync(Guid lessonId, Guid userId, CancellationToken ct);
   ```

Content **не ссылается** на Tests/Assignments — только хранит их id как Guid в поле `data`. Фронт догружает данные параллельными запросами.

---

## 9. UX редактора (преподаватель)

### 9.1. Общая структура `lesson-editor`

- Сверху — название урока + настройки (layout: Scroll/Stepper, обязательность блоков)
- Центр — лента блоков, каждый в своём «контейнере» с кнопками (редактировать, удалить, дублировать, drag-handle)
- Между блоками — разделитель с кнопкой `+`

### 9.2. Добавление блока

Клик по `+` открывает попап-меню с категориями:

```
КОНТЕНТ
  · Текст
  · Видео
  · Аудио
  · Изображение
  · Баннер
  · Файл

УПРАЖНЕНИЯ (автопроверка)
  · Один вариант
  · Несколько вариантов
  · Верно/Неверно
  · Пропуски (ввод)
  · Пропуски (выбор)
  · Банк слов
  · Порядок карточек
  · Сопоставление

УПРАЖНЕНИЯ (ручная/спец. проверка)
  · Открытый ответ
  · Упражнение по коду

СОСТАВНЫЕ
  · Встроенный тест
  · Встроенное задание
```

### 9.3. Редактор блока

У каждого типа — **свой** компонент-редактор, но все наследуют общую секцию настроек:

```
[Контент блока]           ← специфичная форма
───────────────
НАСТРОЙКИ БЛОКА
· Баллы за блок: 1.0
· Обязательный для завершения: ☑
· Подсказка: "..."
· Перемешивать варианты: ☐ (если применимо)
```

### 9.4. Drag & Drop + автосохранение

- Порядок блоков меняется drag-n-drop (уже реализовано)
- Автосохранение при изменении — debounce 1.5 с
- После сохранения — toast «Сохранено»

---

## 10. UX прохождения (студент)

### 10.1. Режим Scroll (по умолчанию для лекций)

- Все блоки на одной странице, вертикально
- Интерактивные блоки имеют кнопку «Проверить» внутри себя
- После проверки блок показывает результат (правильно/неправильно, баллы, подсказка)
- Сверху — общий прогресс `X из Y блоков · Оценка N%`
- Отметка «Пройдено» появляется автоматически при выполнении всех `requiredForCompletion` блоков

### 10.2. Режим Stepper (для практики, как Skyeng)

- Один блок на экране
- Низ экрана:
  ```
  [←]    Страница X из Y | Оценка N.N    [→]
  ```
- Для интерактивного блока кнопка `→` активна только после отправки ответа
- После ответа — сразу показываются результат + фидбэк (если включено)
- На последней странице — экран итогов урока

### 10.3. Фидбэк после ответа

- Правильный: зелёная галочка + «+N баллов»
- Неправильный: красный крестик + (опция) правильный ответ / подсказка
- Открытый ответ: «Отправлено на проверку»
- Код: результаты прогонов тестов

---

## 11. Настройка урока

Добавляется поле в `courses.lessons`:

```
layout  int enum (Scroll=0, Stepper=1)  default Scroll
```

В UI редактора — переключатель наверху урока.

---

## 12. Интеграция с Progress

**Правило автозавершения:**

Урок помечается `Completed` в `progress.lesson_progress`, когда для всех блоков урока с `settings.requiredForCompletion = true` у пользователя:
- Существует попытка в `lesson_block_attempts` со статусом `Graded` и `is_correct = true` (для авто-проверяемых)
- ИЛИ (для Quiz) есть успешная попытка в `tests.test_attempts`
- ИЛИ (для Assignment) есть принятая отправка в `assignments.assignment_submissions`

**Реализация:**

1. Content публикует domain event `BlockAttemptCompleted { BlockId, UserId, LessonId }` после сохранения попытки.
2. Progress подписывается (через MediatR notification handler) и вызывает `IContentReadService.GetCompletionAsync(lessonId, userId)`.
3. Если completion=100% — Progress апсёртит запись в `lesson_progress`.

Альтернатива (проще): при каждом submit attempt Content сам напрямую вызывает `IProgressUpdater.UpdateLessonCompletionAsync` из Shared.

---

## 13. План реализации (этапы)

### Этап A. Фундамент (бэк)
1. Enum `LessonBlockType` со всеми 18 типами
2. C# data-модели (`ILessonBlockData` + 18 конкретных DTO)
3. Перенос LessonBlock в Content (убираем navigation, переносим handlers)
4. Поля `data` (jsonb), `settings` (jsonb) в `LessonBlock`
5. Сущность `LessonBlockAttempt`
6. Контракты в Shared (`ILessonContentCleaner`, `ICourseContentCleaner`, `IContentReadService`)
7. Миграции БД

### Этап B. Автопроверка и API
8. `IBlockGrader` + реализации для каждого проверяемого типа
9. Commands/Queries: SubmitAttempt, GetMyProgress, GetLessonAttempts, ReviewAttempt
10. Контроллеры: `/api/lesson-blocks/{id}/attempts`, `/api/lessons/{id}/my-progress`
11. Интеграция с Progress

### Этап C. Фронтенд типы и сервисы
12. TS дискриминированные юнионы (`blocks.model.ts`)
13. `ContentService` с CRUD блоков
14. `BlockAttemptsService` с submit + прогресс

### Этап D. Фронтенд UI
15. Block viewers (18 компонентов) + host-компонент с ngSwitch
16. Block editors (18 компонентов) + host-компонент
17. Обновление `lesson-editor`: меню `+`, категории, drag & drop
18. Режимы `lesson-view`: Scroll и Stepper
19. Настройка `Lesson.Layout` и UI переключения
20. Прогресс-бар «Страница X из Y · Оценка N%»

### Этап E. Финализация
21. Обновление `implementation_plan.md` и `CLAUDE.md`
22. Smoke-тестирование всех типов блоков
23. Обновление seed-данных (пример урока с разными блоками)

---

## 14. Что не делаем сейчас (out of scope)

- Версионирование блоков (история изменений)
- Библиотека шаблонов блоков / переиспользование между уроками
- A/B-тестирование блоков
- Speech recognition / recording (блок Speaking)
- Collaborative editing (несколько авторов одновременно)

---

## 15. Критерии готовности

- [ ] Все 18 типов блоков создаются и сохраняются
- [ ] Все автопроверяемые типы корректно оцениваются
- [ ] OpenText и CodeExercise выходят на ручную проверку и выставляется оценка
- [ ] Quiz и Assignment блоки открывают соответствующие прохождения
- [ ] Оба режима (Scroll, Stepper) работают
- [ ] Урок автозавершается при 100% required-блоков
- [ ] Прогресс-бар показывает актуальную оценку
- [ ] Удаление урока каскадно чистит блоки + attachments
- [ ] implementation_plan.md и CLAUDE.md отражают фактическую архитектуру

# План переработки модели курса без поломки текущего функционала

Дата: 2026-04-26

## Цель

Сделать курс полноценной верхнеуровневой сущностью платформы.

Курс должен объединять:

- основные данные курса;
- структуру курса;
- уроки;
- тесты;
- задания;
- live-занятия и расписание;
- материалы и файлы;
- прогресс студентов;
- оценки;
- сертификаты;
- оплату и доступ;
- рейтинг;
- отзывы и комментарии.

При этом текущий функционал нельзя ломать. Значит, переработка должна идти через совместимую миграцию: сначала добавляем новую модель рядом со старой, затем постепенно переводим чтение/запись на новую модель, и только потом можно удалять старые обходные решения.

## Текущее состояние

### Что сейчас есть

В модуле `Courses`:

- `Course` хранит метаданные курса: название, описание, дисциплина, преподаватель, цена, статус публикации, дедлайн, сертификат, порядок прохождения.
- `CourseModule` хранит модули курса.
- `Lesson` хранит уроки внутри модуля.
- `CourseEnrollment` хранит запись студента на курс.

В модуле `Content`:

- `LessonBlock` хранит блоки урока.
- Блоки урока могут быть текстом, видео, аудио, изображением, файлом, упражнением, тестом или заданием.

В модуле `Tests`:

- `Test` хранит тест.
- `Question` хранит вопросы теста.
- `TestAttempt` и `TestResponse` хранят прохождение теста студентом.
- У `Test` уже есть nullable `CourseId`.

В модуле `Assignments`:

- `Assignment` хранит задание.
- `AssignmentSubmission` хранит отправку задания студентом.
- У `Assignment` уже есть nullable `CourseId`.

В модуле `Scheduling`:

- `ScheduleSlot` хранит слот занятия.
- У `ScheduleSlot` уже есть nullable `CourseId`.
- `SessionBooking` хранит запись студента на слот.

В модуле `Calendar`:

- `CalendarEvent` хранит календарное событие.
- У события есть nullable `CourseId`, `SourceType`, `SourceId`.

В модуле `Progress`:

- `LessonProgress` хранит прогресс по уроку.
- Сейчас прогресс привязан к `LessonId`, а не напрямую к `CourseId`.

В модуле `Grading`:

- `Grade` хранит оценку студента.
- У оценки есть `CourseId`, источник оценки и комментарий преподавателя.

В модуле `Payments`:

- `CoursePurchase` хранит покупку курса.
- Платежи и выплаты привязаны к `CourseId`, но не являются частью Course aggregate напрямую.

В модуле `Tools`:

- `DictionaryWord` хранит слова глоссария курса через `CourseId`.

### Что сейчас отсутствует или сделано неполно

- Нет единой сущности элемента курса: уроки, тесты, задания и занятия не являются равноправными пунктами структуры.
- `CourseModule` сейчас может содержать только уроки.
- Тесты и задания связаны с курсом, но не имеют нормального места в структуре курса.
- Тесты и задания в уроке подключаются через `LessonBlock`, а не через структуру курса.
- Рейтинг есть в DTO (`CourseListDto.Rating`), но он игнорируется в маппинге и реально не хранится.
- Нет сущности отзыва о курсе.
- Нет сущности комментариев/обсуждений курса.
- Нет единого чеклиста готовности курса.
- Нет статуса готовности у элементов курса и блоков.
- Расписание курса есть только как отдельные слоты, но нет регулярного расписания курса.

## Главная проблема текущей модели

Сейчас курс технически выглядит так:

```text
Course
  -> CourseModule
    -> Lesson
      -> LessonBlock
```

Но продуктово курс должен выглядеть так:

```text
Course
  -> Section / Module
    -> CourseItem: Lesson
    -> CourseItem: Test
    -> CourseItem: Assignment
    -> CourseItem: LiveSession
    -> CourseItem: Resource
```

То есть внутри курса должны быть не только уроки, а любые учебные элементы.

## Целевая концепция

### Course

`Course` остается верхнеуровневой сущностью.

Он отвечает за:

- название;
- описание;
- дисциплину;
- преподавателя;
- уровень;
- обложку;
- цену;
- статус публикации;
- архивирование;
- настройки прохождения;
- настройки оценивания;
- настройки сертификата;
- правила доступа;
- агрегированный рейтинг;
- количество отзывов;
- общую готовность курса.

### CourseSection

`CourseSection` заменяет или постепенно расширяет текущий `CourseModule`.

Секция нужна для группировки элементов курса.

Примеры:

- "Введение";
- "Основы";
- "Практика";
- "Итоговая аттестация";
- "Live-занятия".

На первом этапе можно не переименовывать таблицу `CourseModules`, чтобы не ломать текущий код. В коде можно постепенно вводить новую терминологию `CourseSection`, а физически временно использовать существующую таблицу.

### CourseItem

Новая ключевая сущность.

`CourseItem` - это пункт структуры курса.

Типы элементов:

- `Lesson`;
- `Test`;
- `Assignment`;
- `LiveSession`;
- `Resource`;
- `ExternalLink`;
- `Survey` в будущем.

Пример полей:

```csharp
public class CourseItem : BaseEntity, IAuditableEntity
{
    public Guid CourseId { get; set; }
    public Guid? SectionId { get; set; }
    public CourseItemType Type { get; set; }
    public Guid SourceId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }

    public CourseItemStatus Status { get; set; }
    public bool IsRequired { get; set; }
    public decimal? Points { get; set; }

    public DateTime? AvailableFrom { get; set; }
    public DateTime? Deadline { get; set; }

    public string? CompletionPolicyJson { get; set; }
    public string? VisibilityRulesJson { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

Важно: из-за модульной архитектуры лучше не делать жесткие EF-навигации из `CourseItem` в `Test`, `Assignment` или `ScheduleSlot`. Достаточно хранить `Type + SourceId`, а целостность проверять application-сервисами.

### CourseItemType

```csharp
public enum CourseItemType
{
    Lesson = 0,
    Test = 1,
    Assignment = 2,
    LiveSession = 3,
    Resource = 4,
    ExternalLink = 5
}
```

### CourseItemStatus

```csharp
public enum CourseItemStatus
{
    Draft = 0,
    NeedsContent = 1,
    Ready = 2,
    Published = 3,
    Archived = 4
}
```

### Lesson

`Lesson` остается сущностью урока.

Но урок должен быть не единственным элементом курса, а одним из типов `CourseItem`.

На первом этапе:

- оставить `Lesson.ModuleId`;
- добавить nullable `Lesson.CourseId`;
- заполнить `CourseId` через связь `Lesson -> CourseModule -> Course`;
- старые API продолжают работать.

Позже:

- можно оставить `ModuleId` только для совместимости;
- основным способом положения урока в курсе сделать `CourseItem`.

### LessonBlock

`LessonBlock` остается внутри урока.

Но нужно добавить понятие черновика/готовности.

Проблема сейчас: фронт хочет создать пустой блок, а бэк требует заполненный блок уже на создании.

Нужно:

- разрешить создание пустых блоков как черновиков;
- добавить `ValidationStatus` или `ContentStatus`;
- строгую проверку делать при публикации курса или при переводе блока в статус `Ready`.

Пример:

```csharp
public enum LessonBlockStatus
{
    Draft = 0,
    Ready = 1,
    Invalid = 2
}
```

### Test

`Test` остается в модуле `Tests`.

Изменения:

- `CourseId` постепенно сделать обязательным для тестов, которые создаются в рамках курса.
- Добавить создание теста прямо из Course Builder.
- При добавлении теста в структуру курса создавать `CourseItem` типа `Test`.
- Встроенный тест внутри урока можно оставить как частный случай, но основной путь должен быть через структуру курса или inline-создание из урока.

### Assignment

`Assignment` остается в модуле `Assignments`.

Изменения:

- `CourseId` постепенно сделать обязательным для заданий, которые создаются в рамках курса.
- Добавить создание задания прямо из Course Builder.
- При добавлении задания в структуру курса создавать `CourseItem` типа `Assignment`.
- Встроенное задание внутри урока можно оставить для совместимости.

### LiveSession / Schedule

Сейчас есть `ScheduleSlot`.

Для курса нужно разделить два понятия:

- разовое занятие;
- регулярное расписание.

Минимальный вариант:

- использовать существующий `ScheduleSlot.CourseId`;
- добавить возможность показывать слоты курса в Course Builder;
- при добавлении live-занятия в структуру курса создавать `CourseItem` типа `LiveSession`, где `SourceId = ScheduleSlot.Id`.

Расширенный вариант:

```csharp
public class CourseScheduleRule : BaseEntity, IAuditableEntity
{
    public Guid CourseId { get; set; }
    public string TeacherId { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTimeLocal { get; set; }
    public int DurationMinutes { get; set; }
    public string TimeZoneId { get; set; } = "Europe/Minsk";
    public DateOnly StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }
    public int MaxStudents { get; set; }
    public string? MeetingLink { get; set; }
    public bool IsActive { get; set; }
}
```

Из `CourseScheduleRule` можно генерировать `ScheduleSlot`.

### Rating / Review

Нужно добавить отзывы и рейтинг курса.

Новая сущность:

```csharp
public class CourseReview : BaseEntity, IAuditableEntity
{
    public Guid CourseId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int Rating { get; set; } // 1..5
    public string? Text { get; set; }
    public CourseReviewStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

Правила:

- студент может оставить отзыв только если он записан на курс или купил курс;
- желательно разрешить отзыв после прохождения хотя бы части курса;
- один активный отзыв от одного студента на один курс;
- преподаватель не может оценивать свой курс;
- администратор может скрыть отзыв.

Для производительности можно добавить в `Course`:

- `RatingAverage`;
- `RatingCount`;
- `ReviewsCount`.

Или хранить отдельную таблицу:

```csharp
public class CourseRatingSummary
{
    public Guid CourseId { get; set; }
    public double AverageRating { get; set; }
    public int RatingsCount { get; set; }
    public int ReviewsCount { get; set; }
}
```

### Comments / Discussions

Отзывы и комментарии лучше разделить.

Отзыв - это публичная оценка курса.

Комментарий - это обсуждение внутри курса или урока.

Возможная модель:

```csharp
public class DiscussionThread : BaseEntity, IAuditableEntity
{
    public Guid CourseId { get; set; }
    public Guid? CourseItemId { get; set; }
    public Guid? LessonId { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DiscussionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DiscussionComment : BaseEntity, IAuditableEntity
{
    public Guid ThreadId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

Это можно делать позже. Для первого этапа достаточно `CourseReview`.

## Что должно входить в курс концептуально

### 1. Метаданные

- Название.
- Описание.
- Обложка.
- Дисциплина.
- Уровень.
- Теги.
- Язык.
- Автор/преподаватель.
- Целевая аудитория.
- Предварительные требования.
- Результаты обучения.

### 2. Доступ и коммерция

- Бесплатный/платный курс.
- Цена.
- Валюта.
- Условия доступа.
- Покупки.
- Подписки.
- Возвраты.
- Выплаты преподавателю.

Платежную логику не нужно переносить в `Courses`. Она должна остаться в `Payments`, но курс должен давать понятный read model для цены и доступности.

### 3. Структура

- Секции/модули.
- Элементы курса.
- Порядок элементов.
- Обязательность элементов.
- Дедлайны.
- Доступность по датам.
- Условия открытия следующего элемента.

### 4. Учебный контент

- Уроки.
- Блоки уроков.
- Видео.
- Аудио.
- Текст.
- Файлы.
- Изображения.
- Глоссарий.
- Дополнительные ресурсы.

### 5. Проверочные активности

- Встроенные упражнения в уроках.
- Тесты.
- Задания.
- Открытые ответы.
- Упражнения по коду.
- Ручная проверка.
- Автопроверка.

### 6. Расписание

- Разовые занятия.
- Регулярные занятия.
- Онлайн-ссылки.
- Групповые занятия.
- Индивидуальные занятия.
- Календарные события.

### 7. Прогресс

- Прогресс по урокам.
- Прогресс по элементам курса.
- Общий прогресс курса.
- Дата завершения курса.
- Выполненные обязательные элементы.

### 8. Оценивание

- Оценки за тесты.
- Оценки за задания.
- Оценки за ручные проверки.
- Gradebook.
- Итоговый балл по курсу.

### 9. Социальные элементы

- Рейтинг курса.
- Отзывы.
- Комментарии.
- Обсуждения.
- Вопросы к уроку.

### 10. Публикация и качество

- Статус готовности курса.
- Статус готовности каждого элемента.
- Чеклист публикации.
- Предпросмотр.
- Версионирование в будущем.

## Новая структура данных

Минимальные новые таблицы:

```text
courses.CourseItems
courses.CourseReviews
```

Желательные новые таблицы:

```text
courses.CourseReadinessChecks
courses.CourseRatingSummaries
scheduling.CourseScheduleRules
discussions.DiscussionThreads
discussions.DiscussionComments
```

Минимальные изменения существующих таблиц:

```text
courses.Lessons
  + CourseId nullable

content.LessonBlocks
  + Status
  + ValidationErrorsJson nullable

tests.Tests
  CourseId постепенно сделать обязательным для course-bound тестов

assignments.Assignments
  CourseId постепенно сделать обязательным для course-bound заданий

scheduling.ScheduleSlots
  + CourseItemId nullable
  + LessonId nullable
  + ScheduleRuleId nullable

courses.Courses
  + RatingAverage nullable
  + RatingCount default 0
  + ReviewsCount default 0
  + ReadinessStatus nullable
```

## Новые API

### Course Builder

```text
GET    /api/courses/{courseId}/builder
PATCH  /api/courses/{courseId}/metadata
GET    /api/courses/{courseId}/readiness
POST   /api/courses/{courseId}/publish
```

### Sections

```text
POST   /api/courses/{courseId}/sections
PATCH  /api/course-sections/{sectionId}
DELETE /api/course-sections/{sectionId}
POST   /api/courses/{courseId}/sections/reorder
```

### Course Items

```text
POST   /api/courses/{courseId}/items
PATCH  /api/course-items/{itemId}
DELETE /api/course-items/{itemId}
POST   /api/courses/{courseId}/items/reorder
POST   /api/course-items/{itemId}/mark-ready
POST   /api/course-items/{itemId}/duplicate
```

### Inline creation

```text
POST /api/courses/{courseId}/items/lesson
POST /api/courses/{courseId}/items/test
POST /api/courses/{courseId}/items/assignment
POST /api/courses/{courseId}/items/live-session
POST /api/courses/{courseId}/items/resource
```

Эти endpoint'ы должны создавать и исходную сущность, и `CourseItem`.

Например:

- `POST /items/lesson` создает `Lesson` и `CourseItem`.
- `POST /items/test` создает `Test` и `CourseItem`.
- `POST /items/assignment` создает `Assignment` и `CourseItem`.
- `POST /items/live-session` создает `ScheduleSlot` и `CourseItem`.

### Reviews

```text
GET    /api/courses/{courseId}/reviews
POST   /api/courses/{courseId}/reviews
PATCH  /api/courses/{courseId}/reviews/my
DELETE /api/courses/{courseId}/reviews/my
```

### Discussions в будущем

```text
GET  /api/courses/{courseId}/discussions
POST /api/courses/{courseId}/discussions
POST /api/discussions/{threadId}/comments
```

## Совместимость со старым функционалом

### Что нельзя ломать

- Создание курса.
- Создание модуля.
- Создание урока.
- Редактирование урока.
- Добавление блоков урока.
- Публикация курса.
- Каталог курсов.
- Запись на курс.
- Покупка платного курса.
- Прохождение урока студентом.
- Прогресс уроков.
- Тесты.
- Задания.
- Проверка работ.
- Gradebook.
- Расписание.
- Календарь.
- Отчеты преподавателя.

### Как сохранить совместимость

1. Не удалять существующие таблицы.
2. Не удалять существующие endpoint'ы.
3. Все миграции сначала делать additive-only.
4. Старые endpoint'ы должны писать в старые таблицы и дополнительно синхронизировать новую модель.
5. Новые endpoint'ы должны создавать старые сущности там, где это нужно старому функционалу.
6. Старые DTO должны продолжать возвращать ожидаемую структуру.
7. Новый Course Builder должен работать через новый read model.
8. Старый UI можно оставить за feature flag.

## Миграционный план

### Этап 0. Подготовка и фиксация текущего поведения

Цель: понять, что нельзя сломать.

Задачи:

- Зафиксировать текущую ER-модель.
- Зафиксировать текущие пользовательские сценарии.
- Добавить smoke-тесты на критичные сценарии.
- Добавить backend integration-тесты на основные endpoint'ы.

Минимальные сценарии для тестов:

- преподаватель создает курс;
- преподаватель добавляет модуль;
- преподаватель добавляет урок;
- преподаватель добавляет блок урока;
- преподаватель публикует курс;
- студент записывается на курс;
- студент проходит урок;
- преподаватель создает тест;
- студент проходит тест;
- преподаватель создает задание;
- студент отправляет задание;
- преподаватель проверяет задание;
- преподаватель создает слот расписания;
- студент видит календарное событие;
- студент покупает платный курс.

Результат этапа:

- есть набор тестов, который защищает текущий функционал;
- понятны все места, где используется `CourseId`, `LessonId`, `TestId`, `AssignmentId`.

### Этап 1. Ввести read model Course Builder без изменения БД

Цель: сначала собрать нормальную картину курса из текущих таблиц.

Новый read model:

```csharp
public class CourseBuilderDto
{
    public CourseMetadataDto Course { get; set; }
    public List<CourseSectionDto> Sections { get; set; }
    public List<CourseBuilderItemDto> UnsectionedItems { get; set; }
    public CourseReadinessDto Readiness { get; set; }
}
```

На этом этапе:

- уроки читаются из `CourseModule -> Lessons`;
- тесты читаются из `Tests` по `CourseId`;
- задания читаются из `Assignments` по `CourseId`;
- слоты читаются из `ScheduleSlots` по `CourseId`;
- рейтинг пока возвращается пустым или 0;
- отзывы пока не возвращаются.

Это даст фронту единый endpoint без риска для старого функционала.

### Этап 2. Добавить CourseItems

Цель: сделать уроки, тесты, задания и занятия равноправными элементами курса.

Добавить таблицу:

```text
courses.CourseItems
```

Поля:

- `Id`;
- `CourseId`;
- `SectionId`;
- `Type`;
- `SourceId`;
- `Title`;
- `Description`;
- `OrderIndex`;
- `Status`;
- `IsRequired`;
- `Points`;
- `AvailableFrom`;
- `Deadline`;
- `CompletionPolicyJson`;
- `VisibilityRulesJson`;
- `CreatedAt`;
- `UpdatedAt`.

Backfill:

- для каждого существующего `Lesson` создать `CourseItem` типа `Lesson`;
- `CourseItem.CourseId` взять из `Lesson -> CourseModule -> Course`;
- `CourseItem.SectionId` = `Lesson.ModuleId`;
- `CourseItem.SourceId` = `Lesson.Id`;
- `CourseItem.OrderIndex` = `Lesson.OrderIndex`;
- `CourseItem.Title` = `Lesson.Title`.

Старые уроки после миграции остаются на месте.

### Этап 3. Синхронизировать старые команды с CourseItems

Цель: старый UI продолжает работать, но новая модель уже заполняется.

Изменения:

- `CreateLessonCommandHandler` после создания `Lesson` создает `CourseItem`.
- `UpdateLessonCommandHandler` обновляет snapshot-поля `CourseItem`.
- `DeleteLessonCommandHandler` удаляет или архивирует связанный `CourseItem`.
- `ReorderLessonsCommandHandler` обновляет и старый `Lesson.OrderIndex`, и новый `CourseItem.OrderIndex`.
- `CreateModuleCommandHandler` продолжает создавать `CourseModule`, но в новом read model отображается как section.

Важно: если синхронизация `CourseItem` упала, основную операцию лучше не считать успешной. Иначе структура разъедется.

### Этап 4. Перевести Course Builder на CourseItems

Цель: новый редактор курса должен читать структуру из `CourseItems`.

На этом этапе:

- Course Builder показывает секции;
- внутри секций отображаются элементы разных типов;
- урок открывается в старый lesson editor;
- тест открывается в test editor;
- задание открывается в assignment editor;
- live-занятие открывается в scheduling editor.

Старый редактор структуры можно оставить доступным.

### Этап 5. Inline-создание тестов и заданий

Цель: не уводить пользователя из урока/курса.

Для теста:

- пользователь нажимает "Добавить тест";
- открывается drawer/modal;
- вводит название и вопросы;
- бэк создает `Test`;
- бэк создает `CourseItem` типа `Test`;
- Course Builder обновляется.

Для задания:

- пользователь нажимает "Добавить задание";
- открывается drawer/modal;
- вводит описание, критерии, дедлайн, баллы;
- бэк создает `Assignment`;
- бэк создает `CourseItem` типа `Assignment`.

Старые страницы `/teacher/test/new` и `/teacher/assignment/new` оставить.

### Этап 6. Исправить черновики блоков урока

Цель: блок можно добавить пустым и спокойно заполнить.

Изменения:

- разрешить `LessonBlock` со статусом `Draft`;
- создать более мягкую валидацию на `CreateLessonBlock`;
- строгую валидацию перенести в:
  - `UpdateLessonBlock` при `Status = Ready`;
  - publish checklist;
  - readiness endpoint.

Это исправит текущий конфликт между фронтом и бэком.

### Этап 7. Новый publish checklist

Цель: публикация должна проверять весь курс, а не только модули и пустые уроки.

Проверки:

- есть хотя бы один элемент курса;
- в каждой обязательной секции есть элементы;
- уроки имеют хотя бы один готовый блок;
- блоки не находятся в `Draft`;
- тесты имеют вопросы;
- задания имеют описание и максимальный балл;
- live-занятия имеют дату/время или правило расписания;
- платный курс имеет подключенные выплаты;
- дедлайны не противоречат датам доступности;
- все обязательные элементы имеют completion policy.

Результат:

- `error` блокирует публикацию;
- `warning` не блокирует, но показывает риск;
- каждый issue должен ссылаться на `CourseItemId`, чтобы фронт мог подсветить конкретный элемент.

### Этап 8. Рейтинг и отзывы

Цель: реализовать реальный рейтинг курса.

Добавить:

- `CourseReview`;
- `CourseReviewStatus`;
- `RatingAverage`;
- `RatingCount`;
- `ReviewsCount`.

Логика:

- студент может оставить отзыв после записи на курс;
- лучше ограничить отзыв условием: пройден хотя бы один урок или курс куплен;
- один активный отзыв на студента и курс;
- после создания/изменения/удаления отзыва пересчитать агрегаты;
- каталог курсов должен показывать реальный рейтинг.

### Этап 9. Расписание курса

Цель: сделать расписание частью курса, но не ломать существующие слоты.

Минимально:

- использовать `ScheduleSlot.CourseId`;
- добавить `ScheduleSlot.CourseItemId`;
- Course Builder показывает live-занятия курса.

Расширенно:

- добавить `CourseScheduleRule`;
- генерировать `ScheduleSlot` из правила;
- календарь строить из слотов;
- Course Builder показывает регулярное расписание.

### Этап 10. Прогресс по CourseItem

Цель: прогресс должен учитывать не только уроки.

Сейчас:

- прогресс считается в основном по `LessonProgress`.

Нужно:

- добавить `CourseItemProgress`;
- для `Lesson` прогресс считается по `LessonProgress`;
- для `Test` прогресс считается по успешной/завершенной попытке;
- для `Assignment` прогресс считается по отправке или проверке;
- для `LiveSession` прогресс считается по посещению или завершению занятия;
- общий прогресс курса считается по обязательным `CourseItem`.

Возможная сущность:

```csharp
public class CourseItemProgress : BaseEntity
{
    public Guid CourseItemId { get; set; }
    public Guid CourseId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? Score { get; set; }
    public decimal? MaxScore { get; set; }
}
```

На первом этапе можно не хранить это отдельно, а собирать read model из существующих таблиц. Отдельную таблицу стоит добавлять, когда появятся тесты/задания/live-занятия как обязательные элементы структуры.

### Этап 11. Обсуждения и комментарии

Цель: добавить коммуникацию внутри курса.

Не делать на первом этапе.

Когда делать:

- после стабилизации Course Builder;
- после рейтингов и отзывов;
- после нормального прогресса по CourseItem.

Модель:

- `DiscussionThread`;
- `DiscussionComment`;
- привязка к `CourseId`;
- опциональная привязка к `CourseItemId` или `LessonId`.

## План для фронта

### Новый Course Builder

Экран должен иметь:

- левую панель структуры курса;
- центральную область редактирования выбранного элемента;
- правую панель подсказок/настроек/чеклиста;
- верхнюю панель статуса курса.

### Основные разделы

- Информация.
- Структура.
- Наполнение.
- Проверочные материалы.
- Расписание.
- Публикация.
- Отзывы после публикации.

### В структуре курса должны быть элементы

- Урок.
- Тест.
- Задание.
- Live-занятие.
- Файл/ресурс.

### Старый UI

Старый UI не удалять сразу.

Режимы:

- `CourseBuilderV1` - текущий редактор;
- `CourseBuilderV2` - новый редактор.

Можно включать V2 через feature flag.

## Риски

### Риск 1. Разъезд старой и новой структуры

Если `Lesson` создан, а `CourseItem` не создан, новый редактор не увидит урок.

Решение:

- транзакции;
- repair job;
- диагностический endpoint;
- тесты на синхронизацию.

### Риск 2. Прогресс студентов

Если поменять структуру резко, можно потерять корректный расчет прогресса.

Решение:

- не менять `Lesson.Id`;
- не удалять `LessonProgress`;
- сначала только читать прогресс по старой модели;
- новый `CourseItemProgress` вводить позже.

### Риск 3. Публикация платных курсов

Платный курс связан с выплатами преподавателя.

Решение:

- не трогать текущую payment-логику;
- publish checklist должен продолжать проверять payout readiness.

### Риск 4. Старые тесты и задания без CourseId

`Test.CourseId` и `Assignment.CourseId` nullable.

Решение:

- не делать поле required сразу;
- при inline-создании всегда задавать `CourseId`;
- старые данные мигрировать постепенно;
- для тестов/заданий без курса оставить старый режим.

### Риск 5. Производительность

Course Builder будет собирать данные из нескольких модулей.

Решение:

- отдельный read service;
- батчевые запросы по ID;
- минимальные DTO;
- кэширование readiness/rating summary.

## Порядок реализации

Рекомендуемый порядок:

1. Зафиксировать текущие сценарии тестами.
2. Сделать `CourseBuilderDto` на текущих данных.
3. Добавить `CourseItems`.
4. Backfill существующих уроков в `CourseItems`.
5. Синхронизировать create/update/delete/reorder уроков с `CourseItems`.
6. Перевести новый Course Builder на `CourseItems`.
7. Добавить inline-создание тестов и заданий.
8. Исправить черновики `LessonBlock`.
9. Переписать publish checklist под CourseItem.
10. Добавить рейтинг и отзывы.
11. Добавить Course Schedule Rules.
12. Добавить прогресс по CourseItem.
13. Добавить обсуждения.
14. После стабилизации удалить или скрыть старые обходные сценарии.

## Минимальный MVP переработки

Если делать не все сразу, минимальный полезный объем такой:

1. `CourseItems`.
2. Backfill уроков в `CourseItems`.
3. Новый read endpoint `GET /api/courses/{id}/builder`.
4. Новый frontend Course Builder.
5. Inline-создание теста и задания из курса.
6. Черновики блоков урока.
7. Новый publish checklist.

Рейтинг, отзывы, регулярное расписание и обсуждения можно делать после этого.

## Критерии готовности

### Старый функционал не сломан, если:

- старый список курсов работает;
- старый редактор курса открывается;
- старый редактор урока открывается;
- старые уроки видны;
- блоки уроков работают;
- публикация курса работает;
- студенты видят опубликованные курсы;
- студенты могут записываться и проходить уроки;
- тесты работают;
- задания работают;
- проверки работ работают;
- расписание работает;
- платежи работают.

### Новый функционал готов, если:

- в Course Builder видны уроки, тесты, задания и live-занятия;
- можно добавить элемент любого типа;
- можно переместить элемент внутри курса;
- можно создать тест без ухода на отдельную страницу;
- можно создать задание без ухода на отдельную страницу;
- можно увидеть готовность курса;
- publish checklist подсвечивает конкретные проблемные элементы;
- рейтинг курса считается из реальных отзывов;
- старые API продолжают возвращать совместимые DTO.

## Вывод

Полностью переписывать все сразу не нужно.

Правильный путь:

1. Сначала ввести недостающую абстракцию `CourseItem`.
2. Сделать ее совместимой со старой моделью `CourseModule -> Lesson`.
3. Перевести новый редактор курса на `CourseItem`.
4. Постепенно добавить отзывы, рейтинг, расписание и прогресс по всем элементам курса.

Так мы получим нормальную продуктовую модель курса, но не сломаем текущие уроки, тесты, задания, прогресс, платежи и публикацию.

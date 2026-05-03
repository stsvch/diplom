# Backend-план переработки курса

Дата: 2026-04-26

## Цель

Переработать backend так, чтобы курс стал полноценным учебным пространством, а не только иерархией:

```text
Course -> CourseModule -> Lesson -> LessonBlock
```

Новая модель должна поддерживать:

- уроки;
- тесты;
- задания;
- live-занятия;
- материалы;
- внешние ссылки;
- расписание;
- готовность курса к публикации;
- прогресс по разным типам элементов;
- рейтинг и отзывы.

При этом нельзя сломать текущий функционал:

- создание курсов;
- модули;
- уроки;
- блоки уроков;
- тесты;
- задания;
- расписание;
- календарь;
- прогресс;
- оценки;
- оплату;
- публикацию.

## Ключевое решение

Не переносить все сущности в один модуль `Courses`.

Правильнее оставить текущие bounded contexts:

- `Courses` отвечает за курс, структуру, доступ, публикацию;
- `Content` отвечает за блоки уроков и попытки по блокам;
- `Tests` отвечает за тесты, вопросы и попытки;
- `Assignments` отвечает за задания и отправки;
- `Scheduling` отвечает за занятия и бронирования;
- `Calendar` отвечает за календарные события;
- `Progress` отвечает за прогресс;
- `Grading` отвечает за оценки;
- `Payments` отвечает за покупки, платежи и выплаты.

Но нужно добавить единый слой структуры курса:

```text
CourseItem
```

`CourseItem` - это универсальный пункт программы курса.

Он может указывать на:

- `Lesson`;
- `Test`;
- `Assignment`;
- `ScheduleSlot`;
- `Resource`;
- `ExternalLink`.

Технически связь лучше делать через:

```text
CourseItem.Type + CourseItem.SourceId
```

А не через жесткие EF-связи между разными модулями.

## Что сейчас мешает

### 1. Структура курса умеет хранить только уроки

Сейчас:

```text
Course -> CourseModule -> Lesson
```

Тесты, задания и расписание имеют `CourseId`, но не находятся в структуре курса.

Проблема:

- преподаватель не может собрать курс как маршрут из разных типов элементов;
- тесты и задания ощущаются отдельными сущностями;
- публикация проверяет в основном модули и уроки;
- студент не получает единый маршрут прохождения.

### 2. Права доступа завязаны на старую цепочку

`LessonAccessService` проверяет доступ через:

```text
Lesson -> Module -> Course -> Enrollment
```

Проблема:

- это работает для уроков;
- но не дает универсального доступа к `CourseItem`;
- тесты, задания и слоты проверяются своими путями;
- при появлении универсальной структуры нужен общий access layer.

### 3. Блоки урока нельзя нормально создавать пустыми

`CreateLessonBlockCommandHandler` сразу валидирует данные блока.

Проблема:

- фронт хочет создать пустой блок и дать пользователю заполнить его;
- бэк требует заполненные данные уже при создании;
- это ломает UX конструктора.

### 4. Публикация проверяет слишком мало

Текущий `PublishCourseCommandHandler` проверяет:

- есть ли модули;
- есть ли уроки в модулях;
- есть ли блоки в уроках;
- готов ли преподаватель публиковать платный курс.

Проблема:

- тесты без вопросов не проверяются на уровне курса;
- задания без описания/критериев не проверяются на уровне курса;
- live-занятия не учитываются;
- незаполненные блоки не имеют нормального статуса;
- ошибки не привязаны к универсальному элементу курса.

### 5. Рейтинг есть в DTO, но нет реальной модели

`CourseListDto.Rating` есть, но в AutoMapper он игнорируется.

Проблема:

- рейтинг не считается;
- отзывов курса нет;
- каталог не может показывать реальную обратную связь.

## Целевая backend-модель

### Course

Остается основной сущностью курса.

Нужно оставить:

- название;
- описание;
- дисциплину;
- преподавателя;
- цену;
- статус публикации;
- архивирование;
- порядок прохождения;
- сертификат;
- дедлайн;
- обложку;
- уровень;
- теги.

Желательно добавить позже:

- `RatingAverage`;
- `RatingCount`;
- `ReviewsCount`;
- `ReadinessStatus`.

### CourseModule / CourseSection

На первом этапе не переименовывать таблицу.

`CourseModule` продолжает существовать для совместимости.

В новой модели можно воспринимать его как `CourseSection`.

### CourseItem

Новая сущность в модуле `Courses`.

Примерная модель:

```csharp
public class CourseItem : BaseEntity, IAuditableEntity
{
    public Guid CourseId { get; set; }
    public Guid? ModuleId { get; set; }
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

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

Типы:

```csharp
public enum CourseItemType
{
    Lesson,
    Test,
    Assignment,
    LiveSession,
    Resource,
    ExternalLink
}
```

Статусы:

```csharp
public enum CourseItemStatus
{
    Draft,
    NeedsContent,
    Ready,
    Published,
    Archived
}
```

### LessonBlock

Нужно добавить статус черновика.

Пример:

```csharp
public enum LessonBlockStatus
{
    Draft,
    Ready,
    Invalid
}
```

И добавить поля:

- `Status`;
- `ValidationErrorsJson` или похожий механизм для ошибок;
- возможно `IsRequired`, если обязательность блока должна отличаться от текущих settings.

## Важное архитектурное правило

Модули не должны сильно сцепляться напрямую.

Не надо делать так:

```text
Tests.Application -> Courses.Infrastructure
Assignments.Application -> Courses.Infrastructure
Scheduling.Application -> Courses.Infrastructure
```

Лучше использовать shared-контракты:

```text
EduPlatform.Shared.Application.Contracts
```

Например:

```csharp
public interface ICourseStructureWriter
{
    Task<Guid> UpsertItemAsync(CourseStructureItemUpsert request, CancellationToken ct);
    Task ArchiveItemBySourceAsync(Guid courseId, string sourceType, Guid sourceId, CancellationToken ct);
    Task UpdateItemSnapshotAsync(Guid courseId, string sourceType, Guid sourceId, string title, string? description, CancellationToken ct);
}
```

Или сначала вообще не вызывать это из `Tests`/`Assignments`, а делать inline-создание через новые Course Builder endpoint'ы в `Courses/Host`, которые оркестрируют:

1. создать тест;
2. создать `CourseItem`;
3. вернуть Course Builder DTO.

Для первого этапа безопаснее второй вариант: новые endpoint'ы Course Builder создают и source entity, и `CourseItem`, а старые endpoint'ы продолжают работать как раньше.

## Поэтапный план

## Этап 0. Зафиксировать текущее поведение

Цель: не сломать существующий функционал.

Нужно добавить или проверить тесты на сценарии:

- создание курса;
- обновление курса;
- создание модуля;
- создание урока;
- создание блоков урока;
- публикация курса;
- запись студента на курс;
- прохождение урока;
- создание теста;
- добавление вопросов;
- прохождение теста;
- создание задания;
- отправка задания;
- проверка задания;
- создание слота расписания;
- календарные события по дедлайнам;
- покупка платного курса.

Без этого нельзя безопасно менять модель.

## Этап 1. Добавить Course Builder read model без изменения поведения

Цель: дать фронту единый backend endpoint, который собирает картину курса из текущих таблиц.

Новый endpoint:

```text
GET /api/courses/{courseId}/builder
```

Он должен вернуть:

- данные курса;
- модули/разделы;
- уроки;
- тесты курса;
- задания курса;
- live-занятия курса;
- материалы курса, если есть;
- readiness summary;
- права текущего пользователя.

Пример DTO:

```csharp
public class CourseBuilderDto
{
    public CourseBuilderCourseDto Course { get; set; }
    public List<CourseBuilderSectionDto> Sections { get; set; }
    public List<CourseBuilderItemDto> UnsectionedItems { get; set; }
    public CourseReadinessDto Readiness { get; set; }
}
```

На этом этапе `CourseItem` еще не нужен.

Данные можно собрать так:

- уроки через `CourseModule -> Lessons`;
- тесты через `Test.CourseId`;
- задания через `Assignment.CourseId`;
- live-занятия через `ScheduleSlot.CourseId`.

Зачем это нужно:

- фронт уже сможет проектировать новый Course Builder;
- backend не ломает старые API;
- команда увидит, каких данных не хватает.

## Этап 2. Добавить CourseItems

Цель: начать хранить универсальную структуру курса.

Изменения:

- добавить сущность `CourseItem`;
- добавить `DbSet<CourseItem>` в `ICoursesDbContext`;
- добавить EF-конфигурацию;
- добавить миграцию;
- добавить индексы:
  - `CourseId`;
  - `ModuleId`;
  - `(CourseId, Type, SourceId)` unique;
  - `(CourseId, ModuleId, OrderIndex)`.

Backfill:

- для каждого существующего урока создать `CourseItem` типа `Lesson`;
- `CourseId` взять из `Lesson.Module.CourseId`;
- `ModuleId` взять из `Lesson.ModuleId`;
- `SourceId` = `Lesson.Id`;
- `Title` = `Lesson.Title`;
- `Description` = `Lesson.Description`;
- `OrderIndex` = `Lesson.OrderIndex`;
- `Status` = `Draft` или `Ready` по наличию блоков.

Важно:

- старые таблицы не удалять;
- старые API не менять;
- старый frontend должен работать как раньше.

## Этап 3. Синхронизировать уроки с CourseItems

Цель: если старый UI создает урок, новый Course Builder тоже должен его видеть.

Нужно изменить:

- `CreateLessonCommandHandler`;
- `UpdateLessonCommandHandler`;
- `DeleteLessonCommandHandler`;
- `ReorderLessonsCommandHandler`.

Что делать:

- при создании урока создавать `CourseItem(Lesson)`;
- при переименовании урока обновлять snapshot `CourseItem.Title`;
- при переносе урока в другой модуль обновлять `CourseItem.ModuleId`;
- при удалении урока удалять или архивировать связанный `CourseItem`;
- при reorder обновлять и `Lesson.OrderIndex`, и `CourseItem.OrderIndex`.

Риск:

- операция может частично выполниться.

Решение:

- делать внутри одного `CoursesDbContext` transaction;
- добавить repair job/command для восстановления `CourseItems` из старых уроков.

## Этап 4. Перевести Course Builder read model на CourseItems

Цель: структура курса читается из `CourseItems`, а не только из `Lessons`.

Новый `GET /api/courses/{id}/builder` должен:

- читать `CourseItems`;
- группировать по `ModuleId`;
- для каждого item подтягивать краткую информацию source entity;
- для урока подтягивать `BlocksCount`;
- для теста подтягивать `QuestionsCount`;
- для задания подтягивать `SubmissionsCount` или минимальную мета-информацию;
- для live-занятия подтягивать дату/время.

Нужно добавить read services:

- `ITestReadService` расширить методами по списку ID;
- `IAssignmentReadService` расширить методами по списку ID;
- добавить `IScheduleReadService`, если его нет;
- `IContentReadService` расширить readiness-данными по урокам.

Важно:

- не делать N+1 запросов;
- подтягивать данные батчами.

## Этап 5. Добавить Course Builder write API

Цель: новый frontend должен работать через backend, который понимает структуру курса.

Новые endpoint'ы:

```text
POST   /api/courses/{courseId}/builder/sections
PATCH  /api/courses/{courseId}/builder/sections/{sectionId}
DELETE /api/courses/{courseId}/builder/sections/{sectionId}

POST   /api/courses/{courseId}/builder/items/lesson
POST   /api/courses/{courseId}/builder/items/test
POST   /api/courses/{courseId}/builder/items/assignment
POST   /api/courses/{courseId}/builder/items/live-session
POST   /api/courses/{courseId}/builder/items/resource
POST   /api/courses/{courseId}/builder/items/external-link

PATCH  /api/courses/{courseId}/builder/items/{itemId}
DELETE /api/courses/{courseId}/builder/items/{itemId}
POST   /api/courses/{courseId}/builder/items/reorder
POST   /api/courses/{courseId}/builder/items/{itemId}/duplicate
```

Логика:

- создание lesson-item создает `Lesson` и `CourseItem`;
- создание test-item создает `Test` и `CourseItem`;
- создание assignment-item создает `Assignment` и `CourseItem`;
- создание live-session-item создает `ScheduleSlot` и `CourseItem`;
- создание resource-item создает ресурс/attachment и `CourseItem`;
- external-link может храниться либо как отдельная сущность, либо как data JSON внутри `CourseItem`.

Важный вопрос:

- делать ли write API в `CoursesController` или отдельном `CourseBuilderController`.

Рекомендация:

- создать отдельный `CourseBuilderController`, чтобы не раздувать `CoursesController`.

## Этап 6. Интегрировать тесты и задания в структуру курса

Цель: тесты/задания перестают быть “где-то отдельно” и становятся элементами курса.

Нужно:

- при создании теста через Course Builder создавать `CourseItem(Test)`;
- при создании задания через Course Builder создавать `CourseItem(Assignment)`;
- старые endpoint'ы `/api/tests` и `/api/assignments` оставить;
- для старых endpoint'ов можно либо не создавать `CourseItem`, либо создавать `CourseItem` только если в request передан флаг/sectionId.

Лучший вариант:

- оставить старые endpoint'ы как legacy;
- новый Course Builder использует только новые endpoint'ы;
- позже можно добавить синхронизацию старых endpoint'ов.

Нужно проверить:

- уведомления студентам;
- календарные события дедлайнов;
- права преподавателя;
- удаление/архивация.

## Этап 7. Исправить создание LessonBlock

Цель: разрешить пустые черновые блоки.

Изменения в `Content`:

- добавить `LessonBlockStatus`;
- добавить поле `Status` в `LessonBlock`;
- добавить `ValidationErrorsJson` или возвращать validation result без хранения;
- изменить `CreateLessonBlockCommandHandler`;
- изменить `UpdateLessonBlockCommandHandler`;
- изменить DTO.

Новая логика:

- `CreateLessonBlock` разрешает создать блок с default data и `Status = Draft`;
- строгая валидация не блокирует создание;
- `UpdateLessonBlock` может сохранять черновик даже с неполными данными;
- если пользователь переводит блок в `Ready`, запускается строгая валидация;
- publish checklist проверяет все обязательные блоки.

Нужно решить API:

```text
POST /api/lesson-blocks
PUT  /api/lesson-blocks/{id}
POST /api/lesson-blocks/{id}/mark-ready
POST /api/lesson-blocks/{id}/mark-draft
```

Или добавить `status` в update request.

## Этап 8. Новый readiness / publish checklist

Цель: курс проверяется не только по модулям и урокам.

Нужно создать backend-сервис:

```csharp
CourseReadinessService
```

Он должен возвращать:

- общий статус;
- список ошибок;
- список предупреждений;
- прогресс готовности;
- привязку каждой проблемы к `CourseItemId` или `LessonBlockId`.

Проверки:

- у курса заполнено название;
- есть описание;
- есть обложка, если считаем обязательной;
- есть хотя бы один `CourseItem`;
- у обязательного урока есть готовые блоки;
- у блока нет критических ошибок;
- у теста есть вопросы;
- у задания есть описание и max score;
- у live-занятия есть дата/время/ссылка;
- дедлайн не раньше даты доступности;
- платный курс имеет готовые выплаты;
- если сертификат включен, есть условия завершения.

Endpoint:

```text
GET /api/courses/{courseId}/readiness
```

`PublishCourseCommandHandler` должен использовать этот сервис.

Старый `PublishCourseCommandHandler` нужно не удалить, а переподключить на новую проверку.

## Этап 9. Универсальный access layer для CourseItem

Цель: проверять доступ не только к урокам.

Добавить сервис:

```csharp
CourseAccessService
```

Методы:

```csharp
CanTeacherManageCourseAsync(courseId, teacherId)
CanTeacherManageItemAsync(itemId, teacherId)
CanStudentAccessCourseAsync(courseId, studentId)
CanStudentAccessItemAsync(itemId, studentId)
GetCourseIdByItemAsync(itemId)
```

`LessonAccessService` можно оставить, но постепенно сделать его оберткой над новым сервисом для уроков.

Важно:

- студенты должны видеть только опубликованные/доступные элементы;
- преподаватель должен видеть черновики;
- админ должен видеть все.

## Этап 10. Прогресс по CourseItem

Цель: общий прогресс курса должен учитывать уроки, тесты, задания и live-занятия.

Сначала можно сделать read-only расчет:

- lesson completed из `LessonProgress`;
- test completed из `TestAttempt`;
- assignment completed из `AssignmentSubmission`;
- live-session completed из `SessionBooking` или статуса занятия;
- resource completed позже.

Позже добавить таблицу:

```text
CourseItemProgress
```

Не стоит вводить ее раньше, чем стабилизируется `CourseItem`.

## Этап 11. Рейтинг и отзывы

Цель: реализовать реальный рейтинг курса.

Добавить:

- `CourseReview`;
- `CourseReviewStatus`;
- возможно агрегаты в `Course`.

Endpoint'ы:

```text
GET    /api/courses/{courseId}/reviews
POST   /api/courses/{courseId}/reviews
PUT    /api/courses/{courseId}/reviews/my
DELETE /api/courses/{courseId}/reviews/my
```

Правила:

- студент должен быть записан на курс;
- один активный отзыв на курс от одного студента;
- рейтинг 1..5;
- преподаватель не оценивает свой курс;
- админ может скрывать отзывы.

`CourseListDto.Rating` должен начать заполняться реально.

## Этап 12. Расписание курса

Цель: live-занятия становятся элементами курса.

Сначала:

- использовать текущий `ScheduleSlot.CourseId`;
- добавить `CourseItem(LiveSession)`;
- Course Builder показывает live sessions.

Позже:

- добавить регулярные правила расписания;
- генерировать слоты из правил;
- связать `ScheduleSlot` с `CourseItemId`.

## Этап 13. Обратная совместимость и feature flag

Нужно сохранить:

- текущие controllers;
- текущие routes;
- текущие DTO, которые использует frontend;
- текущие таблицы;
- текущую публикацию;
- текущую оплату.

Новый backend должен работать параллельно.

Рекомендация:

- добавить новый Course Builder API;
- старый frontend оставить рабочим;
- новый frontend включать постепенно;
- после стабилизации удалить legacy только отдельной задачей.

## Что менять в коде по папкам

### `Courses.Domain`

Добавить:

- `CourseItem`;
- `CourseItemType`;
- `CourseItemStatus`;
- позже `CourseReview`.

Опционально позже:

- `CourseReadinessStatus`;
- `CourseReviewStatus`.

### `Courses.Application`

Добавить:

- DTO для Course Builder;
- queries:
  - `GetCourseBuilderQuery`;
  - `GetCourseReadinessQuery`;
- commands:
  - `CreateCourseItemCommand`;
  - `UpdateCourseItemCommand`;
  - `DeleteCourseItemCommand`;
  - `ReorderCourseItemsCommand`;
  - `CreateCourseLessonItemCommand`;
  - `CreateCourseTestItemCommand`;
  - `CreateCourseAssignmentItemCommand`;
  - `CreateCourseLiveSessionItemCommand`;
- сервис readiness:
  - `ICourseReadinessService`;
- синхронизацию уроков с `CourseItem`.

### `Courses.Infrastructure`

Изменить:

- `CoursesDbContext`;
- EF-конфигурацию;
- миграции;
- module registration;
- read/write services для структуры курса.

Добавить:

- backfill migration;
- repair service/command для `CourseItems`.

### `Host`

Добавить:

- `CourseBuilderController`;
- возможно заменить/расширить `LessonAccessService`;
- endpoints для builder/readiness/items/reorder.

### `Content`

Изменить:

- `LessonBlock`;
- `LessonBlockDto`;
- `CreateLessonBlockCommandHandler`;
- `UpdateLessonBlockCommandHandler`;
- validators;
- readiness methods в `IContentReadService`.

Добавить:

- status черновика блока;
- мягкую валидацию черновиков;
- строгую валидацию готовности.

### `Tests`

Оставить текущую модель.

Добавить:

- batch read методы для builder;
- readiness check: есть ли вопросы, max score, дедлайны;
- возможно команду создания теста из Course Builder.

Не ломать:

- попытки тестов;
- grading;
- calendar deadlines;
- notifications.

### `Assignments`

Оставить текущую модель.

Добавить:

- batch read методы для builder;
- readiness check: описание, max score, дедлайн;
- возможно команду создания задания из Course Builder.

Не ломать:

- submissions;
- grading;
- calendar deadlines;
- notifications.

### `Scheduling`

Оставить текущую модель.

Добавить:

- read methods by course/item;
- связь с `CourseItemId` позже;
- readiness check для live-занятия.

### `Progress`

Сначала не менять таблицы.

Добавить позже:

- расчет прогресса по CourseItem;
- возможно `CourseItemProgress`.

### `Grading`

Сначала не менять.

Позже:

- связать grade source с CourseItem;
- показывать оценки в контексте структуры курса.

## Рекомендуемый порядок реализации

### Минимальный безопасный backend MVP

1. Добавить `GET /api/courses/{id}/builder` на старых данных.
2. Добавить `CourseItem`.
3. Backfill существующих уроков в `CourseItem`.
4. Синхронизировать lesson create/update/delete/reorder с `CourseItem`.
5. Перевести builder read model на `CourseItem`.
6. Добавить builder write API для lesson/test/assignment.
7. Исправить `LessonBlock` drafts.
8. Добавить readiness endpoint.
9. Переподключить публикацию на readiness.

### После MVP

10. Добавить live-session как CourseItem.
11. Добавить progress by CourseItem.
12. Добавить CourseReview и реальный рейтинг.
13. Добавить регулярное расписание.
14. Добавить обсуждения/комментарии.

## Проверки перед релизом

Перед включением нового Course Builder нужно проверить:

- старые курсы открываются;
- старые уроки появились как `CourseItem`;
- старый editor урока работает;
- новые элементы создаются;
- reorder не ломает старый порядок уроков;
- публикация показывает корректные ошибки;
- студенты видят опубликованные элементы;
- тесты и задания не потеряли дедлайны, уведомления и календарь;
- оплата платного курса работает;
- отчеты преподавателя не сломались.

## Главный вывод

Backend нужно менять не через полный rewrite, а через слой совместимости:

1. Сначала собрать новый read model курса.
2. Потом добавить `CourseItem`.
3. Потом синхронизировать старые уроки.
4. Потом дать новые builder endpoint'ы.
5. Потом улучшить валидацию, публикацию, прогресс и рейтинг.

Так текущий функционал останется рабочим, а новая модель курса появится постепенно и безопасно.

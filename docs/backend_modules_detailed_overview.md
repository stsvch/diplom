# Детальное описание серверной части EduPlatform

Документ описывает backend по фактическому коду проекта. Его можно использовать как основу для раздела диплома про серверную часть: архитектура, структура solution, слои, модули, ключевые классы, связи между модулями и типовые серверные сценарии.

## 1. Общая архитектура backend

Серверная часть реализована как модульный монолит на ASP.NET Core. Это значит, что приложение запускается как единый backend-процесс, но предметная логика разделена на независимые функциональные модули. Каждый модуль имеет собственные проекты `Domain`, `Application` и `Infrastructure`, а общая точка входа находится в проекте `Host`.

Основная структура:

```text
backend/
  EduPlatform.sln
  src/
    Host/
    AppHost/
    Modules/
      Auth/
      Courses/
      Content/
      Tests/
      Assignments/
      Grading/
      Progress/
      Notifications/
      Calendar/
      Messaging/
      Scheduling/
      Payments/
      Tools/
    Shared/
      EduPlatform.Shared.Domain/
      EduPlatform.Shared.Application/
      EduPlatform.Shared.Infrastructure/
```

`Host` отвечает за HTTP API, подключение middleware, регистрацию модулей, CORS, Swagger, SignalR hubs и автоматическое применение EF Core миграций.

`Modules` содержит предметные модули платформы. Каждый модуль решает свою бизнес-задачу: авторизация, курсы, контент уроков, тесты, задания, оценки, прогресс, уведомления, чат, расписание, платежи и учебные инструменты.

`Shared` содержит общие классы и контракты, которые используются несколькими модулями: базовые сущности, `Result<T>`, `IRepository<T>`, `ValidationBehavior`, межмодульные интерфейсы.

`AppHost` используется для запуска инфраструктуры/оркестрации окружения и не содержит основной предметной логики.

## 2. Архитектурные слои

### 2.1. Domain

Domain-слой хранит сущности, enum-ы и value objects. Здесь находятся классы, описывающие предметную область: `Course`, `LessonBlock`, `Test`, `Assignment`, `Grade`, `ScheduleSlot`, `PaymentAttempt` и другие.

Domain не должен зависеть от контроллеров, базы данных или HTTP. Его задача - описывать бизнес-объекты и их состояние.

### 2.2. Application

Application-слой содержит прикладные сценарии. В проекте используется MediatR, поэтому сценарии оформлены как команды и запросы:

```text
Commands/
Queries/
DTOs/
Interfaces/
Mappings/
Validators/
```

Команда изменяет состояние системы, например `CreateCourseCommand`, `SubmitAttemptCommand`, `GradeSubmissionCommand`. Запрос получает данные без изменения состояния, например `GetCourseByIdQuery`, `GetLessonBlocksQuery`, `GetStudentGradesQuery`.

В Application также находятся DTO, интерфейсы `DbContext`, валидаторы FluentValidation и AutoMapper-профили.

### 2.3. Infrastructure

Infrastructure-слой реализует доступ к данным и внешним сервисам. Здесь находятся EF Core `DbContext`, миграции, сервисы MinIO, Stripe, MongoDB, SignalR-отправители, email-сервис и регистрации зависимостей модулей.

Примеры:

- `CoursesDbContext`
- `ContentDbContext`
- `PaymentsService`
- `StripePaymentGateway`
- `MinioFileStorageService`
- `MongoMessagingRepository`
- `SignalRNotificationSender`

### 2.4. Host

Host-проект связывает все модули. Он содержит:

- REST-контроллеры;
- middleware;
- read-сервисы для dashboard/report/builder экранов;
- DTO для агрегированных API;
- регистрацию модулей в `Program.cs`;
- SignalR endpoints `/hubs/notifications` и `/hubs/chat`.

## 3. Общие backend-паттерны

В коде реально используются следующие подходы:

- Модульный монолит.
- Clean/Onion-подобное разделение на `Domain`, `Application`, `Infrastructure`.
- CQRS на уровне команд и запросов MediatR.
- MediatR как посредник между контроллерами и handler-ами.
- FluentValidation через общий `ValidationBehavior<TRequest,TResponse>`.
- `Result<T>` для возврата успешного результата или ошибки без исключения для ожидаемых бизнес-ошибок.
- AutoMapper для преобразования сущностей в DTO.
- EF Core и отдельные `DbContext` на модуль.
- Repository pattern частично: есть общий `IRepository<T>` и `BaseRepository<T>`, но сложные сценарии часто работают напрямую через интерфейсы `DbContext`.
- Specification pattern в модуле `Courses` для выборок каталога и курсов пользователя.
- SignalR для уведомлений и чата.
- MongoDB для сообщений.
- MinIO для файлов.
- Stripe для платежей.

В проекте не видно полноценного outbox-паттерна, отдельного Unit of Work-класса, domain events или транзакционного MediatR pipeline behavior. Поэтому в дипломе не стоит утверждать, что они используются.

## 4. Как проходит типичный HTTP-запрос

Типовая цепочка:

```text
Frontend
  -> HTTP request
  -> Controller в Host
  -> проверка [Authorize] / ролей / прав доступа
  -> MediatR command/query
  -> Application handler
  -> DbContext / repository / service
  -> сохранение в БД или обращение к внешнему сервису
  -> Result<T> или DTO
  -> HTTP response
```

Пример: студент отправляет попытку прохождения блока урока.

1. Frontend вызывает `POST /api/lesson-blocks/{id}/attempts`.
2. `LessonBlocksController` проверяет JWT и роль `Student`.
3. `LessonAccessService` проверяет, что студент имеет доступ к блоку.
4. Контроллер создаёт `SubmitAttemptCommand`.
5. MediatR передаёт команду в `SubmitAttemptCommandHandler`.
6. Handler загружает `LessonBlock` из `IContentDbContext`.
7. Проверяется тип ответа, лимит попыток и валидность данных.
8. `IBlockGraderRegistry` выбирает grader по типу блока.
9. Создаётся или обновляется `LessonBlockAttempt`.
10. Если все обязательные блоки урока выполнены, вызывается `ILessonProgressUpdater`.
11. Клиент получает `SubmitAttemptResultDto`.

## 5. Shared

`Shared` нужен для общих технических и межмодульных классов.

### 5.1. Shared Domain

| Класс | Назначение |
|---|---|
| `BaseEntity` | Базовая сущность с `Id`. От неё наследуются доменные сущности модулей. |
| `IAuditableEntity` | Контракт для сущностей с `CreatedAt` и `UpdatedAt`. |
| `Result` | Нестрогий результат операции без значения. |
| `Result<T>` | Результат операции со значением или ошибкой. |
| `NotificationType` | Общий enum типов уведомлений. |
| `DeadlineStatus` | Общий enum статусов дедлайна. |
| `CalendarEventType` | Общий enum типов календарных событий. |

### 5.2. Shared Application

| Класс / интерфейс | Назначение |
|---|---|
| `PagedResult<T>` | Универсальная модель постраничного результата. |
| `ApiError` | Стандартизированная HTTP-ошибка для API. |
| `IRepository<T>` | Общий интерфейс репозитория для базовых CRUD-операций. |
| `ValidationBehavior<TRequest,TResponse>` | MediatR pipeline behavior, запускающий FluentValidation перед handler-ом. |
| `IEnrollmentReadService` | Контракт чтения записей студентов на курс. |
| `ICoursePaymentReadService` | Контракт проверки платежного доступа к курсу. |
| `ICourseAccessProvisioningService` | Контракт выдачи доступа к курсу после оплаты. |
| `ICourseAccessRevocationService` | Контракт отзыва доступа к курсу. |
| `IContentReadService` | Контракт чтения информации о контенте. |
| `ILessonContentCleaner` | Контракт очистки контента урока при удалении. |
| `ILessonProgressUpdater` | Контракт обновления прогресса урока из другого модуля. |
| `IGradeRecordWriter` | Контракт записи оценки из теста/задания. |
| `ITestReadService` | Контракт чтения дедлайнов/данных тестов для других модулей. |
| `IAssignmentReadService` | Контракт чтения дедлайнов/данных заданий. |
| `INotificationDispatcher` | Контракт публикации уведомлений. |
| `ICalendarEventPublisher` | Контракт публикации календарных событий. |
| `IChatAdmin` | Контракт административных действий с чатами. |
| `ISubscriptionAllocationReadService` | Контракт чтения данных для распределения подписочных платежей. |
| `ITeacherPayoutReadService` | Контракт проверки готовности преподавателя к платным курсам. |
| `IUserDeletionGuard` | Контракт проверки, можно ли удалить пользователя без нарушения связей. |

### 5.3. Shared Infrastructure

| Класс | Назначение |
|---|---|
| `BaseDbContext` | Общая база для EF Core контекстов модулей. |
| `BaseRepository<T>` | Базовая реализация `IRepository<T>` через EF Core. |

## 6. Host

### 6.1. Program.cs

`Program.cs` выполняет роль композиционного корня приложения:

- подключает `appsettings.Local.json`;
- настраивает Serilog;
- добавляет controllers и JSON enum converter;
- подключает Swagger;
- регистрирует все backend-модули;
- добавляет `ValidationBehavior`;
- настраивает CORS;
- применяет миграции всех EF Core контекстов при запуске;
- подключает middleware;
- мапит контроллеры и SignalR hubs.

### 6.2. Middleware

| Класс | Назначение |
|---|---|
| `ExceptionHandlingMiddleware` | Глобально перехватывает исключения и возвращает единый формат ошибки. |
| `MaintenanceModeMiddleware` | Ограничивает работу API при включённом режиме обслуживания платформы. |

### 6.3. Host services

| Класс | Назначение |
|---|---|
| `LessonAccessService` | Проверяет доступ студента/преподавателя к курсу, модулю, уроку, блоку и попытке. |
| `CourseBuilderReadService` | Собирает агрегированное состояние Course Builder из `Courses`, `Content`, `Tests`, `Assignments`, `Scheduling`. |
| `CourseItemManagementService` | Управляет `CourseItem`: backfill, перемещение, reorder, создание материалов/ссылок, metadata. |
| `CourseItemSyncService` | Синхронизирует `CourseItem` при создании/изменении уроков, тестов, заданий, live-занятий. |
| `CourseReviewService` | Управляет отзывами и пересчитывает агрегированный рейтинг курса. |
| `StudentDashboardReadService` | Собирает данные для dashboard студента. |
| `TeacherDashboardReadService` | Собирает данные для dashboard преподавателя. |
| `TeacherCourseReportReadService` | Собирает отчёт преподавателя по конкретному курсу. |
| `AdminAnalyticsReadService` | Собирает агрегированную аналитику для администратора. |
| `SubscriptionAllocationReadService` | Готовит данные для распределения подписочных платежей. |
| `UserDeletionGuard` | Проверяет зависимости пользователя перед удалением. |

### 6.4. Controllers

| Controller | Основная зона ответственности |
|---|---|
| `AuthController` | Регистрация, вход, refresh, logout, email confirmation, reset password. |
| `UsersController` | Профиль пользователя, смена пароля. |
| `AdminUsersController` | Управление пользователями администратором. |
| `PlatformSettingsController` | Настройки платформы. |
| `CoursesController` | Каталог, CRUD курса, запись, публикация, архивирование. |
| `AdminCoursesController` | Административные действия с курсами. |
| `DisciplinesController` | Дисциплины/категории курсов. |
| `ModulesController` | Разделы курса. |
| `LessonsController` | Уроки курса. |
| `CourseBuilderController` | Управление универсальными элементами Course Builder. |
| `CourseReviewsController` | Отзывы и рейтинг курса. |
| `LessonBlocksController` | Блоки урока, попытки, запуск code exercise. |
| `LessonProgressController` | Проверка попыток и прогресс уроков. |
| `FilesController` | Загрузка, получение и скачивание файлов. |
| `TestsController` | CRUD тестов. |
| `QuestionsController` | Вопросы тестов. |
| `AttemptsController` | Попытки прохождения тестов. |
| `AssignmentsController` | Задания, отправки, оценивание. |
| `GradesController` | Оценки и журнал. |
| `ProgressController` | Прогресс уроков и элементов курса. |
| `NotificationsController` | Уведомления. |
| `CalendarController` | Календарь. |
| `ScheduleController` | Live-занятия и бронирования. |
| `ChatsController` | Управление чатами. |
| `MessagesController` | HTTP-операции с сообщениями. |
| `PaymentsController` | Пользовательские платежи, покупки, подписки, Stripe webhooks. |
| `AdminPaymentsController` | Администрирование платежей, refunds, subscription plans. |
| `ReportsController` | Dashboard/report API. |
| `AdminStatsController` | Статистика администратора. |
| `GlossaryController` | Словарь курса и повторение слов. |
| `HealthController` | Проверка состояния API. |

## 7. Модуль Auth

### 7.1. Назначение

`Auth` отвечает за пользователей, роли, регистрацию, авторизацию, JWT, refresh token, email confirmation, reset password, блокировку пользователей и платформенные настройки.

### 7.2. Domain-классы

| Класс | Поля и роль |
|---|---|
| `ApplicationUser` | Расширяет `IdentityUser`. Поля: `FirstName`, `LastName`, `AvatarUrl`, `CreatedAt`, `UpdatedAt`. |
| `RefreshToken` | Хранит refresh token: `Token`, `UserId`, `ExpiresAt`, `CreatedAt`, `IsRevoked`. |
| `PlatformSetting` | Singleton-настройки платформы: `RegistrationOpen`, `MaintenanceMode`, `PlatformName`, `SupportEmail`, `UpdatedAt`. |
| `UserRole` | Enum ролей: студент, преподаватель, администратор. |

### 7.3. Application-классы

DTO:

- `AuthResponseDto` - access token и пользовательские данные после входа.
- `LoginResultDto` - внутренний результат входа, содержит auth response и refresh token.
- `UserProfileDto` - профиль пользователя.
- `PlatformSettingsDto` - настройки платформы.
- `AdminUserDto` - строка пользователя для админки.
- `UserSummaryDto` - краткая информация о пользователе для поиска/чатов.
- `UserStatsDto` - статистика пользователей.

Интерфейсы:

- `IAuthDbContext` - EF Core контекст авторизации.
- `ITokenService` - генерация access/refresh token и чтение expired token.
- `IEmailService` - отправка email.

Команды:

- `RegisterCommand` / `RegisterCommandHandler` / `RegisterCommandValidator`.
- `LoginCommand` / `LoginCommandHandler` / `LoginCommandValidator`.
- `RefreshTokenCommand` / `RefreshTokenCommandHandler`.
- `LogoutCommand` / `LogoutCommandHandler`.
- `ConfirmEmailCommand` / `ConfirmEmailCommandHandler`.
- `ForgotPasswordCommand` / `ForgotPasswordCommandHandler`.
- `ResetPasswordCommand` / `ResetPasswordCommandHandler` / `ResetPasswordCommandValidator`.
- `ChangePasswordCommand` / `ChangePasswordCommandHandler` / `ChangePasswordCommandValidator`.
- `UpdateProfileCommand` / `UpdateProfileCommandHandler`.
- `UpdatePlatformSettingsCommand` / `UpdatePlatformSettingsCommandHandler`.
- `CreateUserCommand`, `DeleteUserCommand`, `BlockUserCommand`, `UnblockUserCommand`, `ChangeUserRoleCommand` и соответствующие handler-ы для админки.

Запросы:

- `GetProfileQuery` / `GetProfileQueryHandler`.
- `GetAllUsersQuery` / `GetAllUsersQueryHandler`.
- `SearchUsersQuery` / `SearchUsersQueryHandler`.
- `GetPlatformSettingsQuery` / `GetPlatformSettingsQueryHandler`.
- `GetUserStatsQuery` / `GetUserStatsQueryHandler`.

### 7.4. Infrastructure-классы

| Класс | Назначение |
|---|---|
| `AuthDbContext` | EF Core контекст на базе `IdentityDbContext<ApplicationUser>`. |
| `AuthModuleRegistration` | Регистрирует Identity, JWT, MediatR, validators, AutoMapper, seed ролей и администратора. |
| `JwtTokenService` | Генерирует JWT access token, refresh token, извлекает principal из expired token. |
| `SmtpEmailService` | Отправляет email для confirmation/reset сценариев. |

### 7.5. Как работает модуль

При регистрации создаётся `ApplicationUser`, назначается роль и отправляется подтверждение email. При входе `LoginCommandHandler` проверяет пароль через ASP.NET Identity, генерирует JWT и refresh token. Refresh token сохраняется на backend, а клиент получает его через HttpOnly cookie. При каждом JWT-запросе проверяется issuer, audience, подпись, срок жизни, блокировка пользователя и `security_stamp`.

## 8. Модуль Courses

### 8.1. Назначение

`Courses` - центральный модуль учебной структуры. Он отвечает за курсы, дисциплины, разделы курса, уроки, записи студентов, публикацию, архивирование, рейтинг, отзывы и новую модель Course Builder через `CourseItem`.

### 8.2. Domain-классы

| Класс | Поля и роль |
|---|---|
| `Discipline` | Категория курса. Поля: `Name`, `Description`, `ImageUrl`, `Courses`. |
| `Course` | Верхнеуровневая сущность курса. Поля: `DisciplineId`, `TeacherId`, `TeacherName`, `Title`, `Description`, `Price`, `IsFree`, `IsPublished`, `IsArchived`, `OrderType`, `HasGrading`, `HasCertificate`, `Deadline`, `ImageUrl`, `Level`, `Tags`, `RatingAverage`, `RatingCount`, `ReviewsCount`. Связи: `Discipline`, `Modules`, `Items`, `Enrollments`, `Reviews`. |
| `CourseModule` | Раздел курса. Поля: `CourseId`, `Title`, `Description`, `OrderIndex`, `IsPublished`. Связи: `Lessons`, `Items`. |
| `Lesson` | Урок внутри раздела. Поля: `ModuleId`, `Title`, `Description`, `OrderIndex`, `IsPublished`, `Duration`, `Layout`. |
| `CourseItem` | Универсальный пункт структуры Course Builder. Поля: `CourseId`, `ModuleId`, `Type`, `SourceId`, `Title`, `Description`, `Url`, `AttachmentId`, `ResourceKind`, `OrderIndex`, `Status`, `IsRequired`, `Points`, `AvailableFrom`, `Deadline`. |
| `CourseEnrollment` | Запись студента на курс. Поля: `CourseId`, `StudentId`, `EnrolledAt`, `Status`. |
| `CourseReview` | Отзыв и рейтинг студента. Поля: `CourseId`, `StudentId`, `StudentName`, `Rating`, `Comment`. |

Enum-ы:

- `CourseLevel` - уровень курса.
- `CourseOrderType` - режим порядка прохождения.
- `EnrollmentStatus` - статус записи.
- `CourseItemType` - `Lesson`, `Test`, `Assignment`, `LiveSession`, `Resource`, `ExternalLink`.
- `CourseItemStatus` - `Draft`, `NeedsContent`, `Ready`, `Published`, `Archived`.
- `LessonLayout` - `Scroll` или `Stepper`.

### 8.3. Application-классы

DTO:

- `CourseListDto` - карточка курса для списков.
- `CourseDetailDto` - детальная карточка курса.
- `CourseModuleDto` - раздел курса.
- `LessonDto` - урок.
- `DisciplineDto` - дисциплина.
- `AdminCourseDto` - курс для админской таблицы.
- `CourseStatsDto` - статистика по курсам.

Commands:

- `CreateCourseCommand`, `UpdateCourseCommand`, `DeleteCourseCommand`.
- `PublishCourseCommand` и `PublishValidationResult`.
- `ArchiveCourseCommand`, `ForceArchiveCourseCommand`.
- `EnrollCourseCommand`, `UnenrollCourseCommand`.
- `CreateModuleCommand`, `UpdateModuleCommand`, `DeleteModuleCommand`, `ReorderModulesCommand`.
- `CreateLessonCommand`, `UpdateLessonCommand`, `DeleteLessonCommand`, `ReorderLessonsCommand`.
- `CreateDisciplineCommand`, `UpdateDisciplineCommand`, `DeleteDisciplineCommand`.

Queries:

- `GetCourseCatalogQuery`.
- `GetCourseByIdQuery`.
- `GetMyCoursesQuery`.
- `GetAllCoursesAdminQuery`.
- `GetCourseStatsQuery`.
- `GetCourseModulesQuery`.
- `GetModuleLessonsQuery`.
- `GetLessonByIdQuery`.
- `GetDisciplinesQuery`.
- `GetDisciplineByIdQuery`.

Specifications:

- `CourseCatalogSpec` - фильтрация каталога.
- `CourseCatalogCountSpec` - подсчёт каталога.
- `CourseByIdSpec` - загрузка детального курса.
- `TeacherCoursesSpec` - курсы преподавателя.
- `StudentEnrolledCoursesSpec` - курсы студента.

### 8.4. Infrastructure-классы

| Класс | Назначение |
|---|---|
| `CoursesDbContext` | EF Core контекст курсов, разделов, уроков, CourseItem, enrollments, reviews. |
| `CoursesModuleRegistration` | Регистрирует контекст, репозитории, read-сервисы, MediatR, validators, AutoMapper, seed дисциплин. |
| `EnrollmentReadService` | Даёт другим модулям информацию о записях студентов. |
| `CoursePaymentReadService` | Проверяет платежный доступ к курсам. |
| `CourseAccessProvisioningService` | Выдаёт доступ после оплаты. |
| `CourseAccessRevocationService` | Отзывает доступ при отмене/возврате. |

### 8.5. Course Builder

Новая модель курса строится вокруг `CourseItem`. Это не замена `Lesson`, `Test` или `Assignment`, а универсальная оболочка, которая позволяет собрать учебный маршрут из разных элементов.

Связь:

```text
Course
  -> CourseModule
    -> CourseItem(Type = Lesson, SourceId = Lesson.Id)
    -> CourseItem(Type = Test, SourceId = Test.Id)
    -> CourseItem(Type = Assignment, SourceId = Assignment.Id)
    -> CourseItem(Type = LiveSession, SourceId = ScheduleSlot.Id)
    -> CourseItem(Type = Resource / ExternalLink)
```

Host-классы Course Builder:

- `CourseBuilderReadService` - собирает единый builder DTO из нескольких БД.
- `CourseItemSyncService` - создаёт/обновляет `CourseItem` при изменении source-сущности.
- `CourseItemManagementService` - управляет порядком, секциями, ресурсами, ссылками и metadata.
- `CourseBuilderController` - API для операций с `CourseItem`.
- `CourseBuilderDto`, `CourseBuilderItemDto`, `CourseBuilderReadinessDto` - модели ответа для frontend Course Builder.

### 8.6. Как работает модуль

Преподаватель создаёт курс, разделы и уроки через команды `Courses.Application`. При создании тестов, заданий и live-занятий `CourseItemSyncService` создаёт соответствующие элементы структуры курса. Для публикации курс проверяется через `CourseBuilderReadService`: есть ли описание, разделы, элементы, вопросы в тестах, описание и баллы у заданий, корректное время live-занятий. Если есть блокирующие ошибки, публикация не проходит без `force`.

## 9. Модуль Content

### 9.1. Назначение

`Content` отвечает за наполнение уроков. Основная единица - `LessonBlock`. Блок может быть текстом, видео, аудио, файлом, вопросом, заданием, code exercise и другими типами интерактивного контента.

### 9.2. Domain-классы

| Класс | Поля и роль |
|---|---|
| `LessonBlock` | Блок урока. Поля: `LessonId`, `OrderIndex`, `Type`, `Data`, `Settings`, `CreatedAt`, `UpdatedAt`. |
| `LessonBlockAttempt` | Попытка прохождения блока. Поля: `BlockId`, `UserId`, `Answers`, `Score`, `MaxScore`, `IsCorrect`, `NeedsReview`, `AttemptsUsed`, `Status`, `SubmittedAt`, `ReviewedAt`, `ReviewerId`, `ReviewerComment`. |
| `Attachment` | Метаданные файла. Поля: `FileName`, `StoragePath`, `FileUrl`, `ContentType`, `FileSize`, `EntityType`, `EntityId`, `UploadedById`. |
| `CodeExerciseRun` | Запуск кода. Поля: `BlockId`, `UserId`, `AttemptId`, `Kind`, `Language`, `Code`, `Ok`, `GlobalError`, `Results`. |

Enum-ы:

- `LessonBlockType`: `Text`, `Video`, `Audio`, `Image`, `Banner`, `File`, `SingleChoice`, `MultipleChoice`, `TrueFalse`, `FillGap`, `Dropdown`, `WordBank`, `Reorder`, `Matching`, `OpenText`, `CodeExercise`, `Quiz`, `Assignment`.
- `LessonBlockAttemptStatus`.
- `CodeExerciseRunKind`.
- `AttachmentEntityType`.

### 9.3. Value Objects

`LessonBlockData` - абстрактный базовый класс данных блока. Используется полиморфная JSON-сериализация через `$type`.

Классы данных блоков:

- `TextBlockData`.
- `VideoBlockData`.
- `AudioBlockData`.
- `ImageBlockData`.
- `BannerBlockData`.
- `FileBlockData`.
- `SingleChoiceBlockData`, `ChoiceOption`.
- `MultipleChoiceBlockData`.
- `TrueFalseBlockData`, `TrueFalseStatement`.
- `FillGapBlockData`, `FillGapSentence`, `FillGapSlot`.
- `DropdownBlockData`, `DropdownSentence`, `DropdownSlot`.
- `WordBankBlockData`, `WordBankSentence`.
- `ReorderBlockData`, `ReorderItem`.
- `MatchingBlockData`, `MatchingItem`, `MatchingPair`.
- `OpenTextBlockData`, `OpenTextLengthUnit`.
- `CodeExerciseBlockData`, `CodeTestCase`.
- `QuizBlockData`.
- `AssignmentBlockData`.
- `LessonBlockSettings`.

Классы ответов:

- `LessonBlockAnswer` - базовый класс ответа.
- `SingleChoiceAnswer`.
- `MultipleChoiceAnswer`.
- `TrueFalseAnswer`, `TrueFalseResponse`.
- `FillGapAnswer`, `FillGapResponse`, `FillGapValue`.
- `DropdownAnswer`.
- `WordBankAnswer`, `WordBankResponse`.
- `ReorderAnswer`.
- `MatchingAnswer`, `MatchingAnswerPair`.
- `OpenTextAnswer`.
- `CodeExerciseAnswer`, `CodeTestCaseResult`.

### 9.4. Application-классы

DTO:

- `LessonBlockDto`.
- `LessonBlockAttemptDto`.
- `SubmitAttemptResultDto`.
- `LessonProgressDto`.
- `AttachmentDto`.
- `CodeExerciseRunDto`.

Commands:

- `CreateLessonBlockCommand`.
- `UpdateLessonBlockCommand`.
- `DeleteLessonBlockCommand`.
- `ReorderBlocksCommand`.
- `SubmitAttemptCommand`.
- `ReviewAttemptCommand`.
- `UploadFileCommand`.
- `DeleteFileCommand`.
- `ExecuteCodeCommand`.

Queries:

- `GetLessonBlocksQuery`.
- `GetLessonAttemptsQuery`.
- `GetLessonProgressQuery`.
- `GetMyAttemptQuery`.
- `GetFileInfoQuery`.
- `GetDownloadUrlQuery`.
- `GetEntityFilesQuery`.

Validation:

- `IBlockDataValidator`.
- `IBlockDataValidatorRegistry`.
- `BlockDataValidationResult`.
- `BlockDataValidatorRegistry`.
- Валидаторы для каждого типа блока: `TextBlockDataValidator`, `VideoBlockDataValidator`, `AudioBlockDataValidator`, `ImageBlockDataValidator`, `BannerBlockDataValidator`, `FileBlockDataValidator`, `SingleChoiceBlockDataValidator`, `MultipleChoiceBlockDataValidator`, `TrueFalseBlockDataValidator`, `FillGapBlockDataValidator`, `DropdownBlockDataValidator`, `WordBankBlockDataValidator`, `ReorderBlockDataValidator`, `MatchingBlockDataValidator`, `OpenTextBlockDataValidator`, `CodeExerciseBlockDataValidator`, `QuizBlockDataValidator`, `AssignmentBlockDataValidator`.

Grading:

- `IBlockGrader`.
- `IBlockGraderRegistry`.
- `GradeResult`.
- `BlockGraderRegistry`.
- `SingleChoiceGrader`, `MultipleChoiceGrader`, `TrueFalseGrader`, `FillGapGrader`, `DropdownGrader`, `WordBankGrader`, `ReorderGrader`, `MatchingGrader`, `OpenTextGrader`, `CodeExerciseGrader`.

### 9.5. Infrastructure-классы

| Класс | Назначение |
|---|---|
| `ContentDbContext` | Хранит `LessonBlocks`, `LessonBlockAttempts`, `Attachments`, `CodeExerciseRuns`. |
| `ContentModuleRegistration` | Регистрирует DbContext, MinIO, validators, graders, MediatR, AutoMapper. |
| `MinioFileStorageService` | Загружает файлы в MinIO, удаляет объекты, создаёт presigned download URL. |
| `ProcessCodeExecutor` | Выполняет кодовые упражнения. |
| `LessonContentCleaner` | Удаляет контент урока при удалении урока. |
| `ContentReadService` | Даёт другим модулям агрегированную информацию о контенте. |

### 9.6. Как работает модуль

Преподаватель создаёт блок урока через `CreateLessonBlockCommand`. Handler проверяет, что `Data.Type` совпадает с `LessonBlockType`, запускает специализированный validator и сохраняет блок с новым `OrderIndex`.

Студент отправляет ответ через `SubmitAttemptCommand`. Handler проверяет тип ответа, лимит попыток, запускает grader. Для `CodeExercise` сначала выполняется код и сохраняется `CodeExerciseRun`. Результат сохраняется в `LessonBlockAttempt`. Если все обязательные блоки выполнены, вызывается `ILessonProgressUpdater`.

## 10. Модуль Tests

### 10.1. Назначение

`Tests` отвечает за отдельные тесты курса: создание теста, вопросы, варианты ответов, попытки прохождения, сохранение ответов, автоматическую и ручную проверку.

### 10.2. Domain-классы

| Класс | Поля и роль |
|---|---|
| `Test` | Тест. Поля: `CourseId`, `Title`, `Description`, `CreatedById`, `TimeLimitMinutes`, `MaxAttempts`, `Deadline`, `ShuffleQuestions`, `ShuffleAnswers`, `ShowCorrectAnswers`, `MaxScore`, `Questions`. |
| `Question` | Вопрос теста. Поля: `TestId`, `Type`, `Text`, `Points`, `OrderIndex`, `AnswerOptions`. |
| `AnswerOption` | Вариант ответа. Поля: `QuestionId`, `Text`, `IsCorrect`, `OrderIndex`, `MatchingPairValue`. |
| `TestAttempt` | Попытка теста. Поля: `TestId`, `StudentId`, `AttemptNumber`, `StartedAt`, `CompletedAt`, `Score`, `Status`, `Responses`. |
| `TestResponse` | Ответ на вопрос. Поля: `AttemptId`, `QuestionId`, `SelectedOptionIds`, `TextAnswer`, `IsCorrect`, `Points`, `TeacherComment`. |

Enum-ы:

- `QuestionType`.
- `AttemptStatus`.

### 10.3. Application-классы

DTO:

- `TestDto`.
- `TestDetailDto`.
- `QuestionDto`.
- `AnswerOptionDto`.
- `TestAttemptStartDto`.
- `TestAttemptDto`.
- `TestAttemptDetailDto`.
- `TestResponseDto`.
- `StudentQuestionDto`.
- `StudentAnswerOptionDto`.

Commands:

- `CreateTestCommand`, `UpdateTestCommand`, `DeleteTestCommand`.
- `AddQuestionCommand`, `UpdateQuestionCommand`, `DeleteQuestionCommand`, `ReorderQuestionsCommand`.
- `StartAttemptCommand`, `SaveAnswerCommand`, `SubmitAttemptCommand`.
- `GradeResponseCommand`.

Queries:

- `GetTestByIdQuery`.
- `GetMyTestsQuery`.
- `GetTestSubmissionsQuery`.
- `GetAttemptQuery`.
- `GetMyAttemptsQuery`.

### 10.4. Infrastructure-классы

| Класс | Назначение |
|---|---|
| `TestsDbContext` | Хранит тесты, вопросы, ответы, попытки. |
| `TestsModuleRegistration` | Регистрирует DbContext, MediatR, validators, AutoMapper, read-service. |
| `TestReadService` | Даёт другим модулям данные тестов, например для дедлайнов и Course Builder. |

### 10.5. Как работает модуль

Преподаватель создаёт тест и вопросы. Студент запускает попытку через `StartAttemptCommand`, сохраняет ответы через `SaveAnswerCommand`, завершает через `SubmitAttemptCommand`. Автоматически проверяемые вопросы оцениваются сразу, а открытые ответы могут требовать ручной проверки через `GradeResponseCommand`.

## 11. Модуль Assignments

### 11.1. Назначение

`Assignments` отвечает за практические задания: создание задания преподавателем, отправка ответа студентом, ручное оценивание, возврат на доработку.

### 11.2. Domain-классы

| Класс | Поля и роль |
|---|---|
| `Assignment` | Задание. Поля: `CourseId`, `Title`, `Description`, `Criteria`, `Deadline`, `MaxAttempts`, `MaxScore`, `CreatedById`, `Submissions`. |
| `AssignmentSubmission` | Отправка студента. Поля: `AssignmentId`, `StudentId`, `AttemptNumber`, `Content`, `SubmittedAt`, `Status`, `Score`, `TeacherComment`, `GradedAt`, `GradedById`. |
| `SubmissionStatus` | Enum статусов отправки. |

### 11.3. Application-классы

DTO:

- `AssignmentDto`.
- `AssignmentDetailDto`.
- `SubmissionDto`.
- `SubmissionDetailDto`.

Commands:

- `CreateAssignmentCommand`.
- `UpdateAssignmentCommand`.
- `DeleteAssignmentCommand`.
- `SubmitAssignmentCommand`.
- `GradeSubmissionCommand`.

Queries:

- `GetAssignmentByIdQuery`.
- `GetMyAssignmentsQuery`.
- `GetMySubmissionsQuery`.
- `GetPendingSubmissionsQuery`.
- `GetSubmissionsQuery`.
- `GetAssignmentSubmissionsQuery`.

Infrastructure:

- `AssignmentsDbContext`.
- `AssignmentsModuleRegistration`.
- `AssignmentReadService`.

### 11.4. Как работает модуль

Преподаватель создаёт задание с описанием, критериями, дедлайном, лимитом попыток и максимальным баллом. Студент отправляет `AssignmentSubmission`. Преподаватель проверяет отправку через `GradeSubmissionCommand`: выставляет балл, комментарий или возвращает работу на доработку.

## 12. Модуль Grading

### 12.1. Назначение

`Grading` отвечает за единый журнал оценок. Он агрегирует оценки из тестов, заданий и ручных действий преподавателя.

### 12.2. Domain-классы

| Класс | Поля и роль |
|---|---|
| `Grade` | Оценка студента. Поля: `StudentId`, `CourseId`, `SourceType`, `TestAttemptId`, `AssignmentSubmissionId`, `Title`, `Score`, `MaxScore`, `Comment`, `GradedAt`, `GradedById`. |
| `GradeSourceType` | Enum источника оценки: ручная, тест, задание и т.д. |

### 12.3. Application-классы

DTO:

- `GradeDto`.
- `StudentGradesDto`.
- `GradebookDto`.
- `GradebookStatsDto`.

Commands:

- `CreateGradeCommand`.
- `UpdateGradeCommand`.
- `DeleteGradeCommand`.

Queries:

- `GetStudentGradesQuery`.
- `GetCourseGradebookQuery`.
- `GetGradebookStatsQuery`.

Interfaces:

- `IGradingDbContext`.
- `IExportService`.

Infrastructure:

- `GradingDbContext`.
- `GradingModuleRegistration`.
- `GradeRecordWriter`.
- `ExcelExportService`.
- `PdfExportService`.

### 12.4. Как работает модуль

Оценка может появиться вручную или через межмодульный контракт `IGradeRecordWriter`, например после проверки теста или задания. Преподаватель видит gradebook по курсу, студент - свои оценки. Экспорт реализован через Excel/PDF сервисы.

## 13. Модуль Progress

### 13.1. Назначение

`Progress` хранит прохождение уроков и универсальных элементов курса. После добавления `CourseItem` модуль стал поддерживать не только `LessonProgress`, но и `CourseItemProgress`.

### 13.2. Domain-классы

| Класс | Поля и роль |
|---|---|
| `LessonProgress` | Прогресс конкретного урока. Поля: `LessonId`, `StudentId`, `IsCompleted`, `CompletedAt`. |
| `CourseItemProgress` | Прогресс универсального элемента курса. Поля: `CourseId`, `CourseItemId`, `SourceId`, `ItemType`, `StudentId`, `IsCompleted`, `CompletedAt`. |

### 13.3. Application-классы

DTO:

- `LessonProgressDto`.
- `CourseProgressDto`.
- `CourseItemProgressDto`.
- `MyProgressDto`.

Commands:

- `CompleteLessonCommand`.
- `UncompleteLessonCommand`.

Queries:

- `GetCourseProgressQuery`.
- `GetMyProgressQuery`.

Infrastructure:

- `ProgressDbContext`.
- `ProgressModuleRegistration`.
- `LessonProgressUpdater`.

### 13.4. Как работает модуль

Урок может быть завершён явно через API или автоматически из `Content`, когда студент выполнил все обязательные блоки. `LessonProgressUpdater` реализует межмодульный контракт `ILessonProgressUpdater`. Универсальный прогресс `CourseItemProgress` нужен для будущего прохождения тестов, заданий, live-занятий и материалов в одной структуре курса.

## 14. Модуль Notifications

### 14.1. Назначение

`Notifications` хранит уведомления и отправляет их пользователю в real-time через SignalR.

### 14.2. Domain-классы

| Класс | Поля и роль |
|---|---|
| `Notification` | Уведомление. Поля: `UserId`, `Type`, `Title`, `Message`, `IsRead`, `LinkUrl`, `CreatedAt`. |

### 14.3. Application-классы

DTO:

- `NotificationDto`.

Commands:

- `CreateNotificationCommand`.
- `MarkAsReadCommand`.
- `MarkAllAsReadCommand`.
- `DeleteNotificationCommand`.

Queries:

- `GetUserNotificationsQuery`.
- `GetUnreadCountQuery`.

Interfaces:

- `INotificationsDbContext`.
- `INotificationSender`.

Infrastructure:

- `NotificationsDbContext`.
- `NotificationsModuleRegistration`.
- `NotificationHub`.
- `SignalRNotificationSender`.
- `NotificationPublisher`.

### 14.4. Как работает модуль

Другие модули создают уведомления через `INotificationDispatcher`. Уведомление сохраняется в PostgreSQL и отправляется пользователю через `NotificationHub`. Hub авторизован и добавляет соединение пользователя в группу по `UserId`.

## 15. Модуль Calendar

### 15.1. Назначение

`Calendar` отвечает за календарные события пользователя: дедлайны, занятия, события курса и другие учебные даты.

### 15.2. Domain-классы

| Класс | Поля и роль |
|---|---|
| `CalendarEvent` | Событие календаря. Поля: `UserId`, `CourseId`, `Title`, `Description`, `EventDate`, `EventTime`, `Type`, `SourceType`, `SourceId`, `CreatedAt`. |

### 15.3. Application-классы

DTO:

- `CalendarEventDto`.

Commands:

- `CreateCalendarEventCommand`.
- `DeleteCalendarEventCommand`.

Queries:

- `GetMonthEventsQuery`.
- `GetUpcomingEventsQuery`.

Interfaces:

- `ICalendarDbContext`.

Infrastructure:

- `CalendarDbContext`.
- `CalendarModuleRegistration`.
- `CalendarEventPublisher`.

### 15.4. Как работает модуль

События создаются напрямую через `CalendarController` или через межмодульный контракт `ICalendarEventPublisher`. Например, другое действие системы может создать событие дедлайна или занятия. Пользователь получает события за месяц или ближайшие события.

## 16. Модуль Messaging

### 16.1. Назначение

`Messaging` отвечает за чаты и сообщения. В отличие от большинства модулей, он хранит данные в MongoDB, потому что сообщения удобнее хранить как документы.

### 16.2. Domain-документы

| Класс | Поля и роль |
|---|---|
| `ChatDocument` | Документ чата. Поля: `Id`, `Type`, `CourseId`, `CourseName`, `ParticipantIds`, `Participants`, `OwnerId`, `HiddenBy`, `IsArchived`, `LastMessage`, `LastMessageAt`, `CreatedAt`. |
| `ParticipantInfo` | Участник чата: `UserId`, `Name`. |
| `MessageDocument` | Документ сообщения. Поля: `Id`, `ChatId`, `SenderId`, `SenderName`, `Text`, `Attachments`, `SentAt`, `ReadBy`, `IsEdited`. |
| `MessageAttachment` | Вложение сообщения: `FileName`, `FileUrl`, `ContentType`, `FileSize`. |

### 16.3. Application-классы

DTO:

- `ChatDto`.
- `MessageDto`.
- `ParticipantDto`.
- `AttachmentDto`.
- `SendMessageDto`.

Interfaces:

- `IMessagingRepository`.
- `IChatBroadcaster`.
- `IChatConnectionTracker`.

Infrastructure:

- `MongoMessagingRepository`.
- `ChatHub`.
- `SignalRChatBroadcaster`.
- `ChatConnectionTracker`.
- `ChatAdminService`.
- `MessagingModuleRegistration`.

### 16.4. Как работает модуль

При подключении к `ChatHub` пользователь добавляется в группу `user_{userId}` и во все группы своих чатов `chat_{chatId}`. При отправке сообщения hub проверяет, что пользователь участник чата, сохраняет `MessageDocument` в MongoDB, обновляет `LastMessage`, рассылает сообщение через SignalR и создаёт уведомление получателям через `INotificationDispatcher`.

## 17. Модуль Scheduling

### 17.1. Назначение

`Scheduling` отвечает за live-занятия, слоты расписания и бронирования студентов.

### 17.2. Domain-классы

| Класс | Поля и роль |
|---|---|
| `ScheduleSlot` | Слот занятия. Поля: `TeacherId`, `TeacherName`, `CourseId`, `CourseName`, `Title`, `Description`, `StartTime`, `EndTime`, `IsGroupSession`, `MaxStudents`, `Status`, `MeetingLink`, `Bookings`. |
| `SessionBooking` | Бронирование слота студентом. Поля: `SlotId`, `StudentId`, `StudentName`, `BookedAt`, `Status`. |
| `SlotStatus` | Enum статуса слота. |
| `BookingStatus` | Enum статуса бронирования. |

### 17.3. Application-классы

DTO:

- `ScheduleSlotDto`.
- `CreateSlotDto`.
- `BookingDto`.

Commands:

- `CreateSlotCommand`.
- `UpdateSlotCommand`.
- `CancelSlotCommand`.
- `CompleteSlotCommand`.
- `BookSlotCommand`.
- `CancelBookingCommand`.

Queries:

- `GetTeacherSlotsQuery`.
- `GetAvailableSlotsQuery`.
- `GetMyBookingsQuery`.
- `GetSlotByIdQuery`.

Infrastructure:

- `SchedulingDbContext`.
- `SchedulingModuleRegistration`.
- `SchedulingMappingProfile`.

### 17.4. Как работает модуль

Преподаватель создаёт слот занятия. Если слот связан с курсом, он может быть отражён в Course Builder как `CourseItem` типа `LiveSession`. Студент видит доступные слоты и бронирует их. Слот может быть отменён или завершён преподавателем.

## 18. Модуль Payments

### 18.1. Назначение

`Payments` отвечает за платные курсы, подписки, Stripe checkout, Stripe webhooks, сохранённые payment methods, refunds, disputes, teacher payouts и распределение подписочных платежей между преподавателями.

### 18.2. Domain-классы

| Класс | Роль |
|---|---|
| `PaymentAttempt` | Попытка оплаты курса через Stripe. Хранит курс, студента, сумму, валюту, provider ids, статус и ошибку. |
| `CoursePurchase` | Факт покупки курса. Связывает курс, студента и `PaymentAttempt`. |
| `UserPaymentProfile` | Stripe customer пользователя. |
| `PaymentMethodRef` | Сохранённая карта/payment method: brand, last4, expiry, default flag. |
| `RefundRecord` | Возврат платежа, сумма, причина, provider refund id, влияние на расчёты преподавателя. |
| `DisputeRecord` | Stripe dispute/chargeback и его финансовое влияние. |
| `TeacherPayoutAccount` | Stripe connected account преподавателя, статусы onboarding/charges/payouts. |
| `TeacherSettlement` | Расчёт суммы преподавателя за покупку курса. |
| `PayoutRecord` | Запись выплаты преподавателю. |
| `SubscriptionPlan` | Тариф подписки. |
| `SubscriptionPaymentAttempt` | Попытка оплаты подписки. |
| `UserSubscription` | Активная/отменённая подписка пользователя. |
| `SubscriptionInvoice` | Invoice подписки из Stripe. |
| `SubscriptionAllocationRun` | Запуск распределения подписочного платежа. |
| `SubscriptionAllocationLine` | Строка распределения платежа между курсом/преподавателем. |
| `ProcessedWebhookEvent` | Защита от повторной обработки Stripe webhook. |

Enum-ы:

- `PaymentAttemptStatus`.
- `CoursePurchaseStatus`.
- `RefundRecordStatus`.
- `DisputeRecordStatus`.
- `TeacherPayoutAccountStatus`.
- `TeacherSettlementStatus`.
- `PayoutRecordStatus`.
- `SubscriptionPlan`-связанные enum-ы: `SubscriptionBillingInterval`, `UserSubscriptionStatus`, `SubscriptionPaymentAttemptStatus`, `SubscriptionInvoiceStatus`, `SubscriptionAllocationRunStatus`.

### 18.3. Application-классы

DTO:

- `CourseCheckoutSessionDto`.
- `SubscriptionCheckoutSessionDto`.
- `PaymentAttemptDto`.
- `CoursePurchaseDto`.
- `PaymentMethodRefDto`.
- `RefundRecordDto`.
- `DisputeRecordDto`.
- `TeacherPayoutAccountDto`.
- `TeacherSettlementDto`.
- `TeacherSettlementSummaryDto`.
- `PayoutRecordDto`.
- `SubscriptionPlanDto`.
- `SubscriptionPaymentAttemptDto`.
- `UserSubscriptionDto`.
- `SubscriptionInvoiceDto`.
- `TeacherSubscriptionAllocationDto`.
- `AdminPaymentRecordDto`.
- `AdminSubscriptionAllocationRunDto`.
- `AdminSubscriptionAllocationLineDto`.

Interfaces:

- `IPaymentsDbContext`.
- `IPaymentsService`.
- `IPaymentProviderGateway`.

Provider records:

- `ProviderTeacherAccountResult`.
- `ProviderCheckoutSessionRequest`.
- `ProviderSubscriptionCheckoutSessionRequest`.
- `ProviderCheckoutSessionResult`.
- `ProviderRefundRequest`.
- `ProviderRefundResult`.
- `ProviderTransferRequest`.
- `ProviderTransferResult`.
- `ProviderPaymentMethodSnapshot`.
- `ProviderChargeSnapshot`.
- `StripeWebhookEvent`.

### 18.4. Infrastructure-классы

| Класс | Назначение |
|---|---|
| `PaymentsDbContext` | EF Core контекст всех платежных сущностей. |
| `PaymentsModuleRegistration` | Регистрирует DbContext, `PaymentsService`, `StripePaymentGateway`, options. |
| `PaymentsOptions` | Общие настройки платежей. |
| `StripeOptions` | Настройки Stripe: secret key, webhook secret, country и т.д. |
| `StripePaymentGateway` | Обёртка над Stripe API: checkout, refund, transfer, account onboarding, webhook parse. |
| `PaymentsService` | Главный прикладной сервис платежного модуля. |

### 18.5. Методы PaymentsService

`PaymentsService` содержит основные платежные сценарии:

- `CreateCourseCheckoutAsync`.
- `CreateSubscriptionCheckoutAsync`.
- `HandleStripeWebhookAsync`.
- `GetPaymentAttemptAsync`.
- `MarkPaymentAttemptCanceledAsync`.
- `GetMyPaymentHistoryAsync`.
- `GetMyPurchasesAsync`.
- `GetMyPaymentMethodsAsync`.
- `RemoveMyPaymentMethodAsync`.
- `GetMySubscriptionsAsync`.
- `GetMySubscriptionHistoryAsync`.
- `GetMySubscriptionInvoicesAsync`.
- `GetActiveSubscriptionPlansAsync`.
- `CreateSubscriptionPlanAsync`.
- `UpdateSubscriptionPlanAsync`.
- `CreateTeacherOnboardingLinkAsync`.
- `CreateTeacherDashboardLinkAsync`.
- `GetTeacherPayoutAccountAsync`.
- `IsTeacherReadyForPaidCoursesAsync`.
- `GetTeacherSettlementSummaryAsync`.
- `GetTeacherSettlementsAsync`.
- `RequestTeacherPayoutAsync`.
- `CreateAdminRefundAsync`.
- `GetAdminPaymentRecordsAsync`.
- `GetAdminSubscriptionAllocationRunsAsync`.

### 18.6. Как работает модуль

Покупка курса начинается с `CreateCourseCheckoutAsync`. Backend создаёт `PaymentAttempt`, вызывает Stripe checkout и возвращает клиенту checkout URL/session id. Stripe после оплаты отправляет webhook. `HandleStripeWebhookAsync` проверяет событие, защищается от повторной обработки через `ProcessedWebhookEvent`, обновляет `PaymentAttempt`, создаёт `CoursePurchase`, выдаёт доступ к курсу через `ICourseAccessProvisioningService` и формирует расчёты для преподавателя.

Подписки работают похожим образом, но вместо покупки курса создаётся `SubscriptionPaymentAttempt`, затем `UserSubscription`, `SubscriptionInvoice` и allocation records. Выплаты преподавателям идут через `TeacherSettlement`, `SubscriptionAllocationLine` и `PayoutRecord`.

## 19. Модуль Tools

### 19.1. Назначение

`Tools` содержит учебные вспомогательные инструменты. Сейчас основной инструмент - словарь/глоссарий курса с персональным прогрессом повторения слов.

### 19.2. Domain-классы

| Класс | Поля и роль |
|---|---|
| `DictionaryWord` | Слово курса. Поля: `CourseId`, `Term`, `Translation`, `Definition`, `Example`, `Tags`, `CreatedById`, `ProgressEntries`. |
| `UserDictionaryProgress` | Прогресс пользователя по слову. Поля: `WordId`, `UserId`, `IsKnown`, `ReviewCount`, `HardCount`, `RepeatLaterCount`, `LastReviewedAt`, `LastOutcome`, `NextReviewAt`. |
| `DictionaryReviewOutcome` | Enum результата повторения слова. |

### 19.3. Application и Infrastructure

DTO:

- `DictionaryWordDto`.
- `UpsertDictionaryWordDto`.

Interfaces:

- `IToolsDbContext`.
- `IGlossaryService`.

Infrastructure:

- `ToolsDbContext`.
- `ToolsModuleRegistration`.
- `GlossaryService`.

### 19.4. Как работает модуль

Преподаватель или пользователь создаёт слова курса. Студент отмечает слово как известное или проходит review-сессию. `GlossaryService` обновляет `UserDictionaryProgress`, считает повторения и планирует `NextReviewAt`.

## 20. Связи между модулями

Модули не должны напрямую знать всю внутреннюю реализацию друг друга, поэтому часть связей вынесена в `Shared.Application.Contracts`.

Основные связи:

| Откуда | Куда | Через что |
|---|---|---|
| `Content` | `Progress` | `ILessonProgressUpdater` |
| `Tests` / `Assignments` | `Grading` | `IGradeRecordWriter` |
| `Courses` | `Payments` | `ICoursePaymentReadService`, `ITeacherPayoutReadService` |
| `Payments` | `Courses` | `ICourseAccessProvisioningService`, `ICourseAccessRevocationService` |
| `Messaging` | `Notifications` | `INotificationDispatcher` |
| `Scheduling` / `Assignments` / `Tests` | `Calendar` | `ICalendarEventPublisher` |
| `Host CourseBuilder` | `Courses`, `Content`, `Tests`, `Assignments`, `Scheduling` | прямое агрегирующее чтение через DbContext |

## 21. Что лучше показывать на диаграммах

### 21.1. Диаграмма классов

Лучший вариант для основной UML-диаграммы:

```text
Course
CourseModule
CourseItem
Lesson
LessonBlock
LessonBlockData
LessonBlockSettings
LessonBlockAttempt
Attachment
CourseEnrollment
CourseReview
```

Эта диаграмма показывает новую модель курса и объясняет, как курс связан с наполнением уроков.

### 21.2. Диаграмма последовательности

Лучший серверный сценарий:

```text
Student -> LessonBlocksController -> LessonAccessService -> MediatR
        -> SubmitAttemptCommandHandler -> IContentDbContext
        -> IBlockGraderRegistry -> LessonBlockAttempt
        -> ILessonProgressUpdater -> Response
```

Это хороший сценарий для диплома, потому что он показывает авторизацию, проверку доступа, MediatR, бизнес-логику, оценивание и обновление прогресса.

## 22. Краткий вывод для диплома

Backend EduPlatform построен как модульный монолит. Такой подход позволяет держать систему в одном deployable-приложении, но при этом разделять предметную логику по модулям. Основная учебная модель находится в модулях `Courses` и `Content`: `Courses` отвечает за структуру курса, а `Content` - за наполнение уроков и интерактивные блоки. Новая сущность `CourseItem` расширяет модель курса и позволяет Course Builder работать не только с уроками, но и с тестами, заданиями, live-занятиями, материалами и внешними ссылками.

Остальные модули дополняют учебный процесс: `Tests` и `Assignments` отвечают за контроль знаний, `Grading` - за журнал оценок, `Progress` - за прохождение, `Notifications` и `Messaging` - за коммуникацию, `Scheduling` и `Calendar` - за занятия и события, `Payments` - за монетизацию, `Tools` - за дополнительные учебные инструменты.


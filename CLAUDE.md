# Правила написания кода

## Общие принципы

- Придерживаться принципов SOLID, DRY, KISS.
- Не создавать файлы и абстракции «про запас» — только то, что нужно сейчас.
- Общий (shared) код, используемый несколькими модулями, выносится в отдельную библиотеку (Shared).
- Тесты на текущем этапе не пишутся.

### Запрещённые практики

- **Никаких магических Guid/id.** Не использовать `Guid.Empty`, `"00000000-0000-0000-0000-000000000000"` или другие литеральные «заглушки» как значение реальной связи. Если у сущности ещё нет id — файл/данные держатся в памяти до создания родителя, затем загружаются с настоящим id. Это гарантирует отсутствие orphan-записей и корректные FK.
- **Никаких tacit defaults для обязательных ссылок.** Если поле в БД nullable — используй `null`, если не nullable — откладывай сохранение до появления настоящего id.
- **Двухшаговое сохранение для вложений нового объекта:** (1) создать родителя → получить id, (2) загрузить вложение с этим id, (3) при необходимости — обновить родителя ссылкой на вложение.

---

## Backend (C# / ASP.NET Core)

### Именование

- **PascalCase** — классы, методы, свойства, интерфейсы, enums.
- **camelCase** — локальные переменные, параметры методов.
- **_camelCase** — приватные поля класса.
- Интерфейсы начинаются с `I` (например, `ICourseService`).
- Async-методы заканчиваются на `Async` (например, `GetCoursesAsync`).

### Архитектура — модульный монолит

Серверная часть — модульный монолит. Каждый бизнес-модуль (Auth, Courses, Tests и т.д.) — это самостоятельный модуль со своей внутренней структурой по Onion Architecture. Модули изолированы друг от друга и взаимодействуют через контракты (интерфейсы), определённые в Shared.

Внутри каждого модуля — Onion Architecture:
- **Domain** — сущности, value objects, доменные интерфейсы, доменные события.
- **Application** — бизнес-логика, CQRS (commands, queries, handlers), DTOs, validators, маппинг-профили.
- **Infrastructure** — реализация репозиториев (EF Core), внешние сервисы, конфигурация БД.

Отдельно от модулей:
- **Shared** — общий код для всех модулей: Result pattern, базовые интерфейсы, extensions, абстракции, общие DTO.
- **Host (API)** — точка входа приложения: composition root, контроллеры, middleware, конфигурация, DI-регистрация всех модулей.

### CQRS

- Commands — операции, изменяющие состояние (создание курса, отправка задания, выставление оценки).
- Queries — операции чтения данных (получение списка курсов, просмотр журнала, отчёты).
- Каждый command/query имеет свой handler.

### Асинхронность

- Все операции с БД, файлами и внешними сервисами — только async/await.
- Не использовать .Result или .Wait() — это блокирует поток.

### Валидация

- Использовать **FluentValidation**.
- Валидаторы создаются для каждого Command/Query DTO.
- Валидация запускается до выполнения handler-а (через pipeline behavior).

### Маппинг

- Использовать **AutoMapper**.
- Профили маппинга создаются внутри каждого модуля (в слое Application).
- Не маппить доменные сущности напрямую в API-ответы — использовать DTO.

### Обработка ошибок

- Использовать **Result pattern** (Result<T>).
- Не бросать исключения для бизнес-ошибок — возвращать Result с ошибкой.
- Исключения — только для непредвиденных ситуаций (инфраструктурные сбои).
- Глобальный middleware для обработки необработанных исключений.

### Логирование

- Логировать ошибки, предупреждения и ключевые бизнес-события.
- Не логировать чувствительные данные (пароли, токены, платёжные данные).

### БД

- Entity Framework Core для PostgreSQL.
- MongoDB driver для чатов/сообщений.
- Миграции через EF Core Migrations.
- Каждый модуль регистрирует свой DbContext (или использует общий с разделением по схемам).
- Не писать сырые SQL-запросы без необходимости.

---

## Frontend (Angular / TypeScript)

### Именование

- **camelCase** — переменные, методы, свойства.
- **PascalCase** — классы, интерфейсы, enums, типы.
- **kebab-case** — имена файлов и папок (например, `course-card.component.ts`).
- Суффиксы файлов: `*.component.ts`, `*.service.ts`, `*.model.ts`, `*.guard.ts`, `*.interceptor.ts`, `*.pipe.ts`, `*.directive.ts`.

### Компоненты

- Использовать **standalone components** (без NgModules).
- Каждый компонент — в своей папке с файлами: `.ts`, `.html`, `.scss`.
- Компоненты должны быть небольшими и выполнять одну задачу.

### State management

- Использовать **Signals** для реактивного состояния.
- Сервисы с Signals для управления состоянием модулей.
- Не использовать NgRx — избыточно для данного проекта.

### Стили

- Препроцессор **SCSS**.
- Стили — **по-компонентно** (encapsulated, в файле компонента).
- **Глобальные стили** — переменные (цвета, шрифты, отступы), миксины, reset, базовая типографика.
- Использовать UI-библиотеку (определить конкретную позже).

### Прочее

- Строгая типизация — не использовать `any` без крайней необходимости.
- HTTP-запросы — через сервисы, не из компонентов напрямую.
- Lazy loading для feature-модулей (маршруты загружаются по требованию).

---

## Docker

- Весь проект контейнеризирован через Docker.
- Docker Compose для запуска всего окружения одной командой (`docker compose up`).
- Сервисы в Docker Compose:
  - **backend** — ASP.NET Core API
  - **frontend** — Angular (dev-сервер или nginx для продакшн)
  - **postgres** — PostgreSQL
  - **mongo** — MongoDB
  - **minio** — MinIO (S3-совместимое хранилище файлов)
- Переменные окружения — через `.env` файл (не коммитится в git).
- Dockerfile для backend — multi-stage build (restore → build → publish).
- Dockerfile для frontend — multi-stage build (install → build → nginx).

---

## Git

- Коммитим напрямую в main.
- Commit message — краткое описание того, что сделано.
- Не коммитить чувствительные данные (.env, ключи, пароли).

---

## Структура проекта

### Backend — модульный монолит

```
/backend
├── /src
│   ├── /Shared                                — общая библиотека для всех модулей
│   │   ├── /Domain                            — базовые сущности, Result<T>, IEntity, ValueObject
│   │   ├── /Application                       — базовые интерфейсы, pipeline behaviors, extensions
│   │   └── /Infrastructure                    — общие инфраструктурные абстракции (IFileStorage, IEmailSender)
│   │
│   ├── /Modules
│   │   ├── /Auth                              — модуль аутентификации и авторизации
│   │   │   ├── /Auth.Domain                   — User, Role, доменные интерфейсы
│   │   │   ├── /Auth.Application              — Commands, Queries, Handlers, DTOs, Validators
│   │   │   └── /Auth.Infrastructure           — EF конфигурация, репозитории, JWT-сервис
│   │   │
│   │   ├── /Courses                           — модуль курсов
│   │   │   ├── /Courses.Domain                — Course, CourseModule, Lesson (с layout Scroll/Stepper), Discipline, Enrollment
│   │   │   ├── /Courses.Application           — CQRS, DTOs, Validators, маппинг
│   │   │   └── /Courses.Infrastructure        — EF конфигурация, репозитории
│   │   │
│   │   ├── /Content                           — модуль блоков урока + вложений
│   │   │   ├── /Content.Domain                — LessonBlock (data jsonb), LessonBlockAttempt, Attachment, 18 типов value objects (Blocks/Answers)
│   │   │   ├── /Content.Application           — CQRS блоков + попыток, Graders (авто-проверка), Validators
│   │   │   └── /Content.Infrastructure        — S3/MinIO, LessonContentCleaner, ContentReadService
│   │   │
│   │   ├── /Tests                             — модуль тестирования
│   │   │   ├── /Tests.Domain                  — Test, Question, AnswerOption, TestAttempt, TestResponse
│   │   │   ├── /Tests.Application
│   │   │   └── /Tests.Infrastructure
│   │   │
│   │   ├── /Assignments                       — модуль заданий
│   │   │   ├── /Assignments.Domain            — Assignment, AssignmentSubmission
│   │   │   ├── /Assignments.Application
│   │   │   └── /Assignments.Infrastructure
│   │   │
│   │   ├── /Grading                           — модуль оценивания
│   │   │   ├── /Grading.Domain                — Grade
│   │   │   ├── /Grading.Application
│   │   │   └── /Grading.Infrastructure
│   │   │
│   │   ├── /Progress                          — модуль прогресса
│   │   │   ├── /Progress.Domain               — LessonProgress
│   │   │   ├── /Progress.Application
│   │   │   └── /Progress.Infrastructure
│   │   │
│   │   ├── /Scheduling                        — модуль занятий с преподавателем
│   │   │   ├── /Scheduling.Domain             — ScheduleSlot, SessionBooking
│   │   │   ├── /Scheduling.Application
│   │   │   └── /Scheduling.Infrastructure
│   │   │
│   │   ├── /Payments                          — модуль платежей и подписок
│   │   │   ├── /Payments.Domain               — Payment, Subscription, UserSubscription
│   │   │   ├── /Payments.Application
│   │   │   └── /Payments.Infrastructure       — Stripe интеграция
│   │   │
│   │   ├── /Messaging                         — модуль чатов и сообщений
│   │   │   ├── /Messaging.Domain              — Chat, Message (модели для MongoDB)
│   │   │   ├── /Messaging.Application
│   │   │   └── /Messaging.Infrastructure      — MongoDB driver, SignalR хабы
│   │   │
│   │   ├── /Notifications                     — модуль уведомлений
│   │   │   ├── /Notifications.Domain          — Notification
│   │   │   ├── /Notifications.Application
│   │   │   └── /Notifications.Infrastructure  — SignalR хабы
│   │   │
│   │   ├── /Calendar                          — модуль календаря
│   │   │   ├── /Calendar.Domain
│   │   │   ├── /Calendar.Application
│   │   │   └── /Calendar.Infrastructure
│   │   │
│   │   ├── /Reports                           — модуль отчётов и аналитики
│   │   │   ├── /Reports.Domain
│   │   │   ├── /Reports.Application
│   │   │   └── /Reports.Infrastructure
│   │   │
│   │   ├── /Tools                             — модуль инструментов по дисциплинам
│   │   │   ├── /Tools.Domain                  — DictionaryWord, CodeExercise, CodeSubmission
│   │   │   ├── /Tools.Application
│   │   │   └── /Tools.Infrastructure
│   │   │
│   │   └── /Admin                             — модуль администрирования
│   │       ├── /Admin.Domain
│   │       ├── /Admin.Application
│   │       └── /Admin.Infrastructure
│   │
│   └── /Host                                  — точка входа приложения (API)
│       ├── /Controllers                       — контроллеры (тонкие, делегируют в модули)
│       ├── /Middleware                         — глобальные middleware (ошибки, логирование)
│       ├── /Configuration                     — DI-регистрация модулей, настройки
│       ├── Program.cs                         — точка запуска
│       └── appsettings.json
│
└── /tests                                     — тесты (на будущее)
```

### Frontend — Angular

```
/frontend
├── /src
│   ├── /app
│   │   ├── /core                              — синглтон-сервисы, работают на уровне всего приложения
│   │   │   ├── /services                      — AuthService, TokenService, NotificationService
│   │   │   ├── /guards                        — AuthGuard, RoleGuard
│   │   │   ├── /interceptors                  — AuthInterceptor, ErrorInterceptor
│   │   │   └── /models                        — глобальные интерфейсы, enums, типы
│   │   │
│   │   ├── /shared                            — переиспользуемые UI-компоненты
│   │   │   ├── /components                    — общие компоненты
│   │   │   │   ├── /course-card
│   │   │   │   ├── /progress-bar
│   │   │   │   ├── /data-table
│   │   │   │   ├── /file-uploader
│   │   │   │   ├── /confirm-dialog
│   │   │   │   ├── /toast-notification
│   │   │   │   └── /search-input
│   │   │   ├── /pipes                         — форматирование дат, текста и т.д.
│   │   │   ├── /directives                    — кастомные директивы
│   │   │   └── /utils                         — утилитарные функции
│   │   │
│   │   ├── /features                          — функциональные модули (lazy loaded)
│   │   │   ├── /auth                          — вход, регистрация, восстановление пароля
│   │   │   │   ├── /components
│   │   │   │   │   ├── /login
│   │   │   │   │   ├── /register
│   │   │   │   │   └── /forgot-password
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── auth.routes.ts
│   │   │   │
│   │   │   ├── /dashboard                     — дашборды (студент, преподаватель, админ)
│   │   │   │   ├── /components
│   │   │   │   │   ├── /student-dashboard
│   │   │   │   │   ├── /teacher-dashboard
│   │   │   │   │   └── /admin-dashboard
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── dashboard.routes.ts
│   │   │   │
│   │   │   ├── /courses                       — каталог, страница курса, редактор курса
│   │   │   │   ├── /components
│   │   │   │   │   ├── /course-catalog
│   │   │   │   │   ├── /course-detail
│   │   │   │   │   ├── /course-editor
│   │   │   │   │   ├── /module-editor
│   │   │   │   │   └── /lesson-viewer
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── courses.routes.ts
│   │   │   │
│   │   │   ├── /lessons                       — просмотр и редактирование уроков
│   │   │   │   ├── /components
│   │   │   │   │   ├── /lesson-viewer
│   │   │   │   │   ├── /lesson-editor
│   │   │   │   │   └── /content-block
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── lessons.routes.ts
│   │   │   │
│   │   │   ├── /tests                         — прохождение и создание тестов
│   │   │   │   ├── /components
│   │   │   │   │   ├── /test-player
│   │   │   │   │   ├── /test-result
│   │   │   │   │   ├── /test-editor
│   │   │   │   │   └── /question-editor
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── tests.routes.ts
│   │   │   │
│   │   │   ├── /assignments                   — задания и сдача работ
│   │   │   │   ├── /components
│   │   │   │   │   ├── /assignment-detail
│   │   │   │   │   ├── /assignment-editor
│   │   │   │   │   ├── /submission-form
│   │   │   │   │   └── /submission-review
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── assignments.routes.ts
│   │   │   │
│   │   │   ├── /grading                       — журнал оценок
│   │   │   │   ├── /components
│   │   │   │   │   ├── /gradebook
│   │   │   │   │   └── /grade-detail
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── grading.routes.ts
│   │   │   │
│   │   │   ├── /scheduling                    — занятия с преподавателем
│   │   │   │   ├── /components
│   │   │   │   │   ├── /schedule-slots
│   │   │   │   │   ├── /booking
│   │   │   │   │   └── /my-sessions
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── scheduling.routes.ts
│   │   │   │
│   │   │   ├── /payments                      — оплата и подписки
│   │   │   │   ├── /components
│   │   │   │   │   ├── /payment-page
│   │   │   │   │   ├── /subscription-list
│   │   │   │   │   └── /payment-history
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── payments.routes.ts
│   │   │   │
│   │   │   ├── /messaging                     — чаты и сообщения
│   │   │   │   ├── /components
│   │   │   │   │   ├── /chat-list
│   │   │   │   │   ├── /chat-window
│   │   │   │   │   └── /message-input
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── messaging.routes.ts
│   │   │   │
│   │   │   ├── /calendar                      — календарь дедлайнов и занятий
│   │   │   │   ├── /components
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── calendar.routes.ts
│   │   │   │
│   │   │   ├── /notifications                 — уведомления
│   │   │   │   ├── /components
│   │   │   │   │   └── /notification-list
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── notifications.routes.ts
│   │   │   │
│   │   │   ├── /tools                         — словарь, редактор кода
│   │   │   │   ├── /components
│   │   │   │   │   ├── /dictionary
│   │   │   │   │   ├── /flashcards
│   │   │   │   │   └── /code-editor
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── tools.routes.ts
│   │   │   │
│   │   │   ├── /reports                       — отчёты и аналитика
│   │   │   │   ├── /components
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── reports.routes.ts
│   │   │   │
│   │   │   ├── /admin                         — панель администратора
│   │   │   │   ├── /components
│   │   │   │   │   ├── /user-management
│   │   │   │   │   ├── /course-moderation
│   │   │   │   │   ├── /discipline-management
│   │   │   │   │   └── /platform-settings
│   │   │   │   ├── /services
│   │   │   │   ├── /models
│   │   │   │   └── admin.routes.ts
│   │   │   │
│   │   │   └── /profile                       — профиль пользователя
│   │   │       ├── /components
│   │   │       ├── /services
│   │   │       ├── /models
│   │   │       └── profile.routes.ts
│   │   │
│   │   ├── /layouts                           — layout-компоненты по ролям
│   │   │   ├── /guest-layout                  — layout без sidebar (landing, login, register)
│   │   │   ├── /student-layout                — sidebar студента + header
│   │   │   ├── /teacher-layout                — sidebar преподавателя + header
│   │   │   └── /admin-layout                  — sidebar администратора + header
│   │   │
│   │   ├── app.component.ts
│   │   ├── app.config.ts
│   │   └── app.routes.ts                      — корневой роутинг
│   │
│   ├── /assets                                — статические ресурсы (иконки, изображения)
│   │
│   ├── /styles                                — глобальные SCSS-стили
│   │   ├── _variables.scss                    — цвета, шрифты, отступы, брейкпоинты
│   │   ├── _mixins.scss                       — миксины (адаптивность, typography)
│   │   ├── _reset.scss                        — сброс/нормализация стилей
│   │   ├── _typography.scss                   — базовые стили текста
│   │   └── styles.scss                        — главный файл, импорт всего
│   │
│   └── /environments                          — конфигурации окружений
│       ├── environment.ts
│       └── environment.prod.ts
│
└── angular.json
```

### Корень проекта

```
/diplom
├── /backend                    — серверная часть
├── /frontend                   — клиентская часть
├── /docs                       — документация
├── docker-compose.yml          — оркестрация всех сервисов
├── .env.example                — пример переменных окружения
├── .gitignore
├── README.md
└── CLAUDE.md                   — правила написания кода
```

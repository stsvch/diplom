# План реализации

План является ориентиром. Перед каждым этапом проводится детальное обсуждение. Отклонения от плана допускаются при обосновании.

---

## Этап 1. Инициализация проекта

**Backend:**
- Создать solution (.sln), проекты: Host (API), Shared (Domain, Application, Infrastructure)
- Настроить подключение к PostgreSQL (EF Core + Npgsql)
- Настроить базовый middleware (обработка ошибок, логирование)
- Реализовать Result<T> pattern в Shared
- Настроить AutoMapper, FluentValidation, MediatR (для CQRS)
- Swagger для документации API

**Frontend:**
- Создать Angular-проект (standalone)
- Настроить SCSS, глобальные стили (variables, reset, typography)
- Создать layouts (guest, student, teacher, admin)
- Настроить роутинг (корневой + lazy loading)
- Подключить UI-библиотеку
- Настроить HTTP-interceptors (заглушки)
- Настроить environments (dev, prod)

**Инфраструктура:**
- Dockerfile для backend
- Dockerfile для frontend
- Docker Compose: backend, frontend, PostgreSQL, MongoDB, MinIO
- Запуск всего окружения одной командой

**Результат:** оба проекта запускаются через docker compose up, фронт отображает пустой layout, бэк отдаёт Swagger.

---

## Этап 2. Аутентификация и пользователи (Auth)

**Backend:**
- Модуль Auth: Domain, Application, Infrastructure
- Сущность User, миграция
- Регистрация (email + пароль, хэширование)
- Вход (выдача JWT-токена), refresh token
- Восстановление пароля
- Middleware авторизации (роли: Admin, Teacher, Student)
- Эндпоинт получения/обновления профиля

**Frontend:**
- Feature /auth: login, register, forgot-password
- AuthService, TokenService
- AuthInterceptor (добавление JWT), ErrorInterceptor
- AuthGuard, RoleGuard
- Редирект по роли после входа
- Страница профиля (базовая)

**Результат:** пользователь может зарегистрироваться, войти, система разграничивает роли.

---

## Этап 3. Дисциплины и курсы (Courses)

**Backend:**
- Модуль Courses: Discipline, Course, CourseModule, Lesson, CourseEnrollment
- CRUD дисциплин (админ)
- CRUD курсов (преподаватель): создание, редактирование, публикация, архивирование
- CRUD модулей и уроков
- Каталог курсов (фильтрация, поиск)
- Запись на курс
- Настройки курса: платный/бесплатный, последовательный/свободный, с оценками/без

**Frontend:**
- Каталог курсов: карточки, фильтры, поиск
- Страница курса (превью): описание, модули, преподаватель, кнопка записи
- Мои курсы (student): список с прогресс-баром
- Редактор курса (teacher): создание, настройки
- Редактор структуры (teacher): модули и уроки, drag & drop
- Управление дисциплинами (admin)

**Результат:** преподаватель создаёт курсы со структурой, студент видит каталог и записывается.

---

## Этап 4. Блочная система контента уроков (Content)

**Архитектура:** модуль Content отвечает за всё наполнение урока — блоки разных типов, их автопроверку, попытки студентов, вложения (Attachment) в MinIO. Модуль Courses знает только про структуру (курс → модуль → урок). Связь через cross-schema FK + контракт `ILessonContentCleaner` в Shared.

**Backend:**
- Модуль Content: LessonBlock (data jsonb + settings jsonb), LessonBlockAttempt, Attachment
- Enum LessonBlockType из 18 типов:
  - Контент: Text, Video, Audio, Image, Banner, File
  - Авто-проверяемые: SingleChoice, MultipleChoice, TrueFalse, FillGap, Dropdown, WordBank, Reorder, Matching
  - Ручная проверка: OpenText, CodeExercise
  - Составные: Quiz, Assignment (ссылки на Tests / Assignments)
- Типизированные value objects для data и answer каждого типа (JsonPolymorphic discriminator)
- `IBlockGrader` registry — 10 реализаций автопроверки
- `IBlockDataValidator` registry — 18 валидаторов корректности data перед публикацией
- Commands: Create/Update/Delete/ReorderBlocks, SubmitAttempt, ReviewAttempt
- Queries: GetLessonBlocks, GetMyAttempt, GetLessonProgress, GetLessonAttempts
- Контракты в Shared: `ILessonContentCleaner`, `IContentReadService`, `ILessonProgressUpdater`
- Интеграция с Progress: при сдаче последнего required-блока урока автоматически помечается Completed
- Загрузка файлов в S3/MinIO (универсальный Attachment с EntityType + EntityId)

**Frontend:**
- ContentService + BlockAttemptsService (HTTP)
- TS discriminated union на каждый тип блока (data + answer)
- 18 viewer-компонентов (один блок → один компонент)
- 18 editor-компонентов с inline-UX (radio для SingleChoice, чипы синонимов для FillGap, палитра для Banner и т.д.)
- BlockHost, BlockInserter (линия + меню), BlockTypeMenu (4 категории с поиском)
- BlockViewerHost / BlockEditorHost — ngSwitch по type
- Lesson editor: добавление через `+`, автосохранение 1.5с, перемещение ↑↓, дублирование
- Lesson view (scroll): sticky прогресс-бар, inline-фидбэк (зелёная/красная граница, баллы, осталось попыток)
- Layout на Lesson (`Scroll` / `Stepper`)

**Результат:** преподаватель собирает урок из 18 типов блоков; студент проходит, получает мгновенный фидбэк; урок автоматически помечается пройденным при выполнении всех обязательных блоков.

См. `docs/lesson_blocks_design.md` и `docs/lesson_editor_ux_design.md`.

---

## Этап 5. Тестирование (Tests)

**Backend:**
- Модуль Tests: Test, Question, AnswerOption, TestAttempt, TestResponse
- Создание теста с настройками (таймер, попытки, дедлайн, перемешивание)
- Типы вопросов: SingleChoice, MultipleChoice, TextInput, Matching, OpenAnswer
- Прохождение теста: начать → отвечать → завершить
- Автопроверка закрытых вопросов, подсчёт баллов
- Отметка открытых вопросов для ручной проверки
- Сохранение попыток, автосохранение ответов

**Frontend:**
- Создание теста (teacher): настройки, вопросы, варианты ответов
- Прохождение теста (student): навигация, таймер, завершение
- Результат теста (student): баллы, детализация
- Проверка открытых вопросов (teacher): ответ студента, оценка, комментарий

**Результат:** полный цикл тестирования работает.

---

## Этап 6. Задания (Assignments)

**Backend:**
- Модуль Assignments: Assignment, AssignmentSubmission
- CRUD заданий: описание, критерии, дедлайн, попытки
- Сдача задания (студент): текст + файлы (через Attachment)
- Проверка (преподаватель): оценка, комментарий, возврат на доработку
- Статусы: Submitted → UnderReview → Graded / ReturnedForRevision
- Контроль количества попыток

**Frontend:**
- Создание задания (teacher)
- Просмотр задания (student): описание, дедлайн, форма сдачи
- Сдача работы (student): текст + файлы, история попыток
- Проверка работ (teacher): список, форма оценивания

**Результат:** полный цикл «задание → сдача → проверка → оценка».

---

## Этап 7. Оценивание и прогресс (Grading + Progress)

**Backend:**
- Модуль Grading: Grade, автоматическое создание записей, ручное редактирование, итоговый балл, экспорт PDF/Excel
- Модуль Progress: LessonProgress, отметка прохождения, расчёт процента завершённости

**Frontend:**
- Журнал оценок (teacher): таблица, редактирование, экспорт
- Журнал оценок (student): мои оценки по курсам
- Прогресс-бар, кнопка «Пройдено»

**Результат:** электронный журнал и отслеживание прогресса работают.

---

## Этап 8. Уведомления и календарь (Notifications + Calendar)

**Backend:**
- Модуль Notifications: генерация при событиях, SignalR для real-time push
- Модуль Calendar: агрегация дедлайнов и событий

**Frontend:**
- Уведомления: иконка с badge, dropdown/страница, фильтрация
- Календарь: дедлайны, занятия
- Real-time обновление через SignalR
- Teacher calendar: ручные custom events, выбор дня, список событий дня, переходы по событиям

**Результат:** пользователь не пропускает важные события.

---

## Этап 9. Чаты и сообщения (Messaging)

**Backend:**
- Модуль Messaging: MongoDB (chats, messages), чат курса, личные сообщения, вложения, SignalR, статус прочтения

**Frontend:**
- Список чатов, окно чата, отправка текста и файлов, индикатор непрочитанных, real-time
- Deep-link в конкретный чат, read receipts, синхронизация unread через SignalR

**Результат:** общение внутри платформы.

---

## Этап 10. Занятия с преподавателем (Scheduling)

**Backend:**
- Модуль Scheduling: ScheduleSlot, SessionBooking, создание слотов, запись, статусы, напоминания

**Frontend:**
- Расписание (teacher): создание слотов, записавшиеся
- Запись (student): доступные слоты, мои занятия, история

**Результат:** студент записывается на занятие.

---

## Этап 11. Подписки и оплата (Payments)

**Backend:**
- Модуль Payments: Payment, Subscription, UserSubscription, интеграция Stripe, проверка доступа, история

**Frontend:**
- Оплата курса (Stripe Checkout), подписки, история платежей

**Результат:** монетизация работает.

---

## Этап 12. Инструменты по дисциплинам (Tools)

**Backend:**
- Модуль Tools: DictionaryWord, CodeExercise, CodeSubmission, CRUD словаря, запуск и проверка кода

**Frontend:**
- Словарь: список, добавление, карточки для заучивания
- Редактор кода: Monaco Editor, запуск, результат

**Результат:** специализированные инструменты для разных дисциплин.

---

## Этап 13. Отчёты и аналитика (Reports)

**Backend:**
- Модуль Reports: агрегация данных, отчёты по студенту/курсу/группе, статистика платформы

**Frontend:**
- Дашборд студента, преподавателя, администратора
- Графики и диаграммы

**Результат:** все роли видят аналитику.

---

## Этап 14. Администрирование (Admin)

**Backend:**
- Модуль Admin: управление пользователями, модерация курсов, настройки платформы

**Frontend:**
- Управление пользователями, модерация курсов, настройки

**Результат:** админ контролирует платформу.

---

## Этап 15. Финализация

- Адаптивная вёрстка (мобильные, планшеты)
- Оптимизация производительности
- Проверка безопасности
- Landing page
- Финальное тестирование всех сценариев
- Документация API

---

## Сводная таблица

| Этап | Название | Зависит от | Статус |
|------|----------|------------|--------|
| 1 | Инициализация проекта | — | Готов |
| 2 | Аутентификация и пользователи | 1 | Готов (+ сидирование админа) |
| 3 | Дисциплины и курсы | 2 | Готов |
| 4 | Блочная система контента | 3 | Готов (18 типов, автопроверка, попытки, интеграция с Progress) |
| 5 | Тестирование | 3, 4 | Готов |
| 6 | Задания | 3, 4 | Готов |
| 7 | Оценивание и прогресс | 4, 5, 6 | Готов (Progress интегрирован с Content) |
| 8 | Уведомления и календарь | 3 | Готов |
| 9 | Чаты и сообщения | 2 | Готов |
| 10 | Занятия с преподавателем | 2, 3 | Готов |
| 11 | Подписки и оплата | 3 | Ожидает |
| 12 | Инструменты по дисциплинам | 3, 4 | Ожидает |
| 13 | Отчёты и аналитика | 7 | Ожидает |
| 14 | Администрирование | 2, 3 | Частично (только Disciplines на фронте) |
| 15 | Финализация | все | Ожидает |

---

## Артефакты проектирования

- `docs/lesson_blocks_design.md` — полная спецификация 18 типов блоков
- `docs/lesson_editor_ux_design.md` — UX/UI редактора и просмотра урока
- `docs/payments_architecture_design.md` — архитектура оплат, выплат преподавателям и подписок

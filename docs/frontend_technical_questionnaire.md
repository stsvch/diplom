# Техническая анкета и описание клиентской части EduPlatform

Документ подготовлен по фактической структуре проекта `frontend` и может использоваться как основа для раздела 4.3 пояснительной записки. Клиентская часть реализована как отдельное Angular SPA-приложение, взаимодействующее с backend через REST API и SignalR.

## 1. Общая информация о приложении

1. Полная тема диплома в проектных материалах формулируется как разработка веб-платформы для организации учебного процесса. В README проект описан как `EduPlatform - Образовательная платформа для организации учебного процесса`.

2. Само программное средство называется `EduPlatform`.

3. Приложение решает задачу организации учебного процесса в электронной среде: создание и публикация курсов, прохождение уроков, выполнение заданий, тестирование, контроль прогресса, ведение журнала оценок, организация занятий, обмен сообщениями, уведомления, работа с файлами и платежами.

4. Основные пользователи системы: гость, студент, преподаватель и администратор. Гость просматривает публичные страницы и каталог. Студент проходит обучение и оплачивает доступ. Преподаватель создает курсы, уроки, тесты и задания. Администратор управляет пользователями, курсами, дисциплинами, платежами, аналитикой и настройками платформы.

5. Через клиентскую часть пользователь выполняет вход, регистрацию, восстановление пароля, просмотр каталога курсов, запись на курс, прохождение уроков и тестов, сдачу заданий, просмотр оценок и прогресса, работу с календарем и расписанием, обмен сообщениями, просмотр уведомлений, оплату курсов или подписок, редактирование профиля.

6. В приложении есть публичные страницы, кабинет студента, кабинет преподавателя и кабинет администратора. Основные разделы: landing page, каталог курсов, страница курса, авторизация, регистрация, дашборды, мои курсы, редактор курса, редактор урока, тесты, задания, журнал оценок, календарь, расписание, сообщения, уведомления, платежи, словарь, профиль, администрирование пользователей, курсов, дисциплин, платежей, аналитики и настроек.

7. Клиентская часть находится в отдельной папке `frontend` и является отдельным веб-приложением. Backend расположен отдельно в папке `backend`.

8. Приложение работает как SPA: переходы между страницами выполняются через Angular Router без полной перезагрузки страницы.

## 2. Технологический стек фронтенда

1. Основной frontend-фреймворк: Angular 20.

2. Используется TypeScript. В `tsconfig.json` включены строгие настройки: `strict`, `noImplicitReturns`, `strictTemplates`, `strictInjectionParameters`.

3. Сборка выполняется средствами Angular CLI и пакета `@angular/build`. Точка входа приложения: `src/main.ts`.

4. Готовая UI-библиотека вроде Material UI, Bootstrap или Ant Design не используется. Интерфейс построен на собственных standalone-компонентах и SCSS-стилях. Для иконок используется `lucide-angular`. Для drag-and-drop используется `@angular/cdk`.

5. Для маршрутизации используется встроенный `@angular/router`.

6. Для запросов к серверу используется Angular `HttpClient`.

7. Отдельная библиотека серверного состояния вроде TanStack Query, RTK Query или SWR не используется. Данные загружаются через сервисы, Observables и Angular signals.

8. Отдельного Redux/Zustand/MobX-хранилища нет. Глобальные и локальные состояния реализованы через Angular services и signals: `AuthService`, `SidebarService`, `ToastService`, `SignalRService`, `ChatSignalRService`, а также локальный `CourseBuilderStore` для конструктора курса.

9. Для форм используются `ReactiveFormsModule`, `FormsModule` и встроенные `Validators`. Отдельные библиотеки валидации вроде Zod/Yup не используются.

10. Дополнительные библиотеки: `@microsoft/signalr` для real-time уведомлений и чатов, `ngx-monaco-editor-v2` и `monaco-editor` для редактора кода, `ngx-tiptap` и `@tiptap/*` для rich text редактора, `lowlight` для подсветки блоков кода, `@angular/cdk` для перетаскивания элементов.

## 3. Структура проекта

Фронтенд расположен в папке `frontend`. Основная структура:

```text
frontend/
  src/
    main.ts
    index.html
    styles.scss
    styles/
      _variables.scss
      _reset.scss
      _typography.scss
      _animations.scss
    environments/
      environment.ts
      environment.prod.ts
    app/
      app.ts
      app.config.ts
      app.routes.ts
      core/
        guards/
        interceptors/
        models/
        services/
      layouts/
        app-layout/
        guest-layout/
        header/
        sidebar/
      shared/
        components/
        directives/
        pipes/
      features/
        admin/
        assignments/
        auth/
        calendar/
        content/
        courses/
        grading/
        messaging/
        notifications/
        payments/
        profile/
        progress/
        public/
        reports/
        scheduling/
        tests/
        tools/
```

1. Структура модульная, с разделением на `core`, `shared`, `layouts` и `features`.

2. Страницы и функциональные модули находятся в `features`. Общие компоненты находятся в `shared/components`. Сервисы, guards, interceptors и базовые модели пользователя/ошибки/файлов находятся в `core`.

3. API-запросы хранятся в сервисах: например `courses.service.ts`, `tests.service.ts`, `assignments.service.ts`, `payments.service.ts`, `admin.service.ts`, `messaging.service.ts`.

4. DTO-типы хранятся рядом с функциональными модулями в папках `models`: `course.model.ts`, `test.model.ts`, `assignment.model.ts`, `payments.model.ts`, `admin.model.ts`, `block-data.model.ts` и т.д.

5. Переиспользуемые UI-компоненты находятся в `src/app/shared/components`: `button`, `input`, `card`, `badge`, `course-card`, `file-uploader`, `file-card`, `progress-bar`, `rich-text-editor`, `rich-text-viewer`, `search-input`, `stats-card`, `toast`, `avatar`, `video-player`, `user-picker`.

6. Архитектура близка к модульной feature-based структуре. Это не строгий Feature-Sliced Design, но проект разделен по функциональным областям.

7. Точка входа приложения: `src/main.ts`. Корневой компонент: `src/app/app.ts`. Конфигурация приложения: `src/app/app.config.ts`. Маршруты: `src/app/app.routes.ts`.

## 4. Маршрутизация и навигация

Маршрутизация описана в `src/app/app.routes.ts`. Все основные страницы загружаются лениво через `loadComponent`, что уменьшает начальный объем загружаемого кода.

1. Основные маршруты:

```text
/                         публичная главная страница
/login                    вход
/register                 регистрация
/forgot-password          восстановление пароля
/confirm-email            подтверждение email
/reset-password           сброс пароля
/catalog                  публичный каталог
/course/:id               публичная страница курса
/messages/:chatId         редирект на чат для авторизованного пользователя

/student/dashboard
/student/courses
/student/catalog
/student/course/:id
/student/lesson/:id
/student/test/:testId/play
/student/test/:testId/result/:attemptId
/student/assignment/:id
/student/grades
/student/calendar
/student/schedule
/student/messages
/student/messages/:chatId
/student/notifications
/student/payments
/student/glossary
/student/profile

/teacher/dashboard
/teacher/courses
/teacher/courses/new
/teacher/courses/create
/teacher/courses/edit/:id
/teacher/courses/:id/editor
/teacher/courses/:id/builder
/teacher/courses/:id/preview
/teacher/lesson-preview/:id
/teacher/lesson/:id/edit
/teacher/lesson/:id/review
/teacher/test/new
/teacher/test/:id/edit
/teacher/test/:testId/submissions
/teacher/test/:testId/grade/:attemptId
/teacher/assignments
/teacher/assignment/new
/teacher/assignment/:id/edit
/teacher/gradebook
/teacher/calendar
/teacher/schedule
/teacher/reports
/teacher/messages
/teacher/messages/:chatId
/teacher/notifications
/teacher/payments
/teacher/glossary
/teacher/profile

/admin/dashboard
/admin/users
/admin/courses
/admin/disciplines
/admin/payments
/admin/analytics
/admin/settings
```

2. Публичные страницы размещены внутри `GuestLayoutComponent`. Защищенные страницы студента, преподавателя и администратора размещены внутри `AppLayoutComponent`.

3. Авторизация перед доступом к защищенным маршрутам реализована через `authGuard`.

4. Проверка роли реализована через `roleGuard`. Для групп маршрутов указано `data: { role: 'Student' }`, `data: { role: 'Teacher' }`, `data: { role: 'Admin' }`.

5. Общий layout авторизованной зоны включает sidebar, header и основной `<router-outlet>`.

6. Вложенные маршруты используются для публичной зоны и для role-based кабинетов.

7. Динамические маршруты используются для курсов, уроков, тестов, заданий и сообщений: `:id`, `:testId`, `:attemptId`, `:chatId`.

8. Отдельной страницы 404 нет. Wildcard-маршрут `**` перенаправляет на главную страницу.

9. Меню формируется через вычисляемый массив `navItems` в `SidebarComponent`. Состав меню зависит от текущей роли.

10. Breadcrumbs в коде не обнаружены. Пользователь понимает текущий раздел по активному пункту меню: используется `RouterLinkActive` и CSS-класс активного элемента.

## 5. Взаимодействие с backend API

1. Клиент обращается к серверу через Angular `HttpClient`.

2. Базовый URL API хранится в `src/environments/environment.ts` и `environment.prod.ts`: `apiUrl: '/api'`.

3. В dev-режиме связь с backend выполняется через `proxy.conf.js`. Переменная окружения `BACKEND_URL` задает адрес backend, по умолчанию `http://localhost:5000`.

4. Единого класса `ApiClient` нет, но есть общий `HttpClient`, подключенный в `app.config.ts` через `provideHttpClient`.

5. Настроены два functional interceptor-а: `errorInterceptor` и `authInterceptor`.

6. `errorInterceptor` преобразует HTTP-ошибки к единому формату `ApiError`.

7. Ошибки обрабатываются централизованно на уровне interceptor-а и дополнительно на уровне компонентов. Для стандартных статусов заданы сообщения: 401 - сессия истекла, 403 - доступ запрещен, 404 - ресурс не найден, 500 - ошибка сервера. При `status === 0` показывается сообщение об отсутствии соединения с сервером.

8. `authInterceptor` автоматически добавляет заголовок `Authorization: Bearer <token>`.

9. Авторизация работает через JWT access token и refresh token через cookie. Access token хранится в `localStorage`, а refresh-запросы выполняются с `withCredentials: true`.

10. Refresh token поддерживается backend-эндпоинтом `/api/auth/refresh`. Frontend вызывает его через `AuthService.refreshToken()`.

11. При ответе 401 interceptor пытается обновить access token и повторить исходный запрос. Если обновление не удалось, выполняется logout и переход на `/login`.

12. Пользовательские сообщения об успехе и ошибках показываются через `ToastService` и компонент `ToastComponent`.

13. Основные backend endpoints, используемые фронтендом:

```text
Auth:
POST /api/auth/login
POST /api/auth/register
GET  /api/auth/confirm-email
POST /api/auth/forgot-password
POST /api/auth/reset-password
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/users/me

Courses:
GET    /api/courses
GET    /api/courses/{id}
GET    /api/courses/my
POST   /api/courses
PUT    /api/courses/{id}
POST   /api/courses/{id}/publish
POST   /api/courses/{id}/archive
DELETE /api/courses/{id}
POST   /api/courses/{id}/enroll
POST   /api/courses/{id}/unenroll

Modules and lessons:
GET    /api/modules/by-course/{courseId}
POST   /api/modules
PUT    /api/modules/{id}
DELETE /api/modules/{id}
POST   /api/modules/reorder
GET    /api/lessons/by-module/{moduleId}
GET    /api/lessons/{id}
POST   /api/lessons
PUT    /api/lessons/{id}
DELETE /api/lessons/{id}
POST   /api/lessons/reorder

Course builder:
GET    /api/courses/{courseId}/builder
POST   /api/courses/{courseId}/builder/backfill
POST   /api/courses/{courseId}/builder/items
PUT    /api/courses/{courseId}/builder/items/{itemId}
PUT    /api/courses/{courseId}/builder/items/{itemId}/metadata
DELETE /api/courses/{courseId}/builder/items/{itemId}
POST   /api/courses/{courseId}/builder/items/{itemId}/move
POST   /api/courses/{courseId}/builder/items/reorder

Content:
GET    /api/lesson-blocks/by-lesson/{lessonId}
POST   /api/lesson-blocks
PUT    /api/lesson-blocks/{id}
DELETE /api/lesson-blocks/{id}
POST   /api/lesson-blocks/reorder
POST   /api/lesson-blocks/{blockId}/execute-code
POST   /api/lesson-blocks/{blockId}/attempts
GET    /api/lesson-blocks/{blockId}/my-attempt
GET    /api/lesson-blocks/{blockId}/my-code-runs
GET    /api/lessons/{lessonId}/my-progress
GET    /api/lessons/{lessonId}/attempts
GET    /api/lessons/{lessonId}/code-runs
POST   /api/lesson-block-attempts/{attemptId}/review

Tests:
POST   /api/tests
GET    /api/tests/my
GET    /api/tests/{id}
PUT    /api/tests/{id}
DELETE /api/tests/{id}
GET    /api/tests/{testId}/submissions
POST   /api/tests/{testId}/questions
PUT    /api/questions/{id}
DELETE /api/questions/{id}
POST   /api/tests/{testId}/questions/reorder
POST   /api/tests/{testId}/start
POST   /api/attempts/{attemptId}/answer
POST   /api/attempts/{attemptId}/submit
GET    /api/attempts/{attemptId}
GET    /api/tests/{testId}/my-attempts
PUT    /api/responses/{responseId}/grade

Assignments:
POST   /api/assignments
GET    /api/assignments/my
GET    /api/assignments/{id}
PUT    /api/assignments/{id}
DELETE /api/assignments/{id}
GET    /api/assignments/{assignmentId}/submissions
GET    /api/assignments/{assignmentId}/my-submissions
GET    /api/assignments/pending
POST   /api/assignments/{assignmentId}/submit
PUT    /api/submissions/{submissionId}/grade

Grading and progress:
POST   /api/grades
PUT    /api/grades/{id}
DELETE /api/grades/{id}
GET    /api/grades/course/{courseId}
GET    /api/grades/course/{courseId}/stats
GET    /api/grades/student/{studentId}
GET    /api/grades/my
GET    /api/grades/course/{courseId}/export/excel
GET    /api/grades/course/{courseId}/export/pdf
POST   /api/progress/lessons/{lessonId}/complete
DELETE /api/progress/lessons/{lessonId}/complete
GET    /api/progress/courses/{courseId}
GET    /api/progress/my
GET    /api/progress/lessons/{lessonId}

Files:
POST   /api/files/upload
GET    /api/files/{id}
GET    /api/files/{id}/download
DELETE /api/files/{id}
GET    /api/files

Notifications and messaging:
GET    /api/notifications
GET    /api/notifications/unread-count
PUT    /api/notifications/{id}/read
PUT    /api/notifications/read-all
DELETE /api/notifications/{id}
GET    /api/chats
GET    /api/chats/{chatId}
GET    /api/chats/unread-count
POST   /api/chats/direct
POST   /api/chats/course
GET    /api/chats/{chatId}/messages
POST   /api/chats/{chatId}/messages
PUT    /api/chats/{chatId}/read
POST   /api/chats/{chatId}/hide
DELETE /api/chats/{chatId}
POST   /api/chats/{chatId}/participants
DELETE /api/chats/{chatId}/participants/{participantId}
PUT    /api/messages/{messageId}
DELETE /api/messages/{messageId}
GET    /api/users/search

Calendar and schedule:
GET    /api/calendar/events
GET    /api/calendar/upcoming
POST   /api/calendar/events
DELETE /api/calendar/events/{id}
POST   /api/schedule/slots
GET    /api/schedule/slots/my
PUT    /api/schedule/slots/{id}
POST   /api/schedule/slots/{id}/cancel
POST   /api/schedule/slots/{id}/complete
GET    /api/schedule/slots/{id}
GET    /api/schedule/slots/{id}/bookings
GET    /api/schedule/available
POST   /api/schedule/slots/{id}/book
DELETE /api/schedule/slots/{id}/book
GET    /api/schedule/my-bookings
```

## 6. Сервисный слой

1. Сервисный слой есть. Компоненты в основном работают не напрямую с `HttpClient`, а через сервисы.

2. Основные сервисы:

```text
core/services/auth.service.ts
core/services/users.service.ts
core/services/file.service.ts
core/services/sidebar.service.ts
core/services/signalr.service.ts
core/services/chat-signalr.service.ts
features/courses/services/courses.service.ts
features/courses/services/disciplines.service.ts
features/courses/course-builder/services/course-builder.service.ts
features/content/services/content.service.ts
features/content/services/block-attempts.service.ts
features/tests/services/tests.service.ts
features/assignments/services/assignments.service.ts
features/grading/services/grading.service.ts
features/progress/services/progress.service.ts
features/calendar/services/calendar.service.ts
features/scheduling/services/scheduling.service.ts
features/reports/services/reports.service.ts
features/payments/services/payments.service.ts
features/notifications/services/notifications.service.ts
features/messaging/services/messaging.service.ts
features/admin/services/admin.service.ts
features/tools/services/glossary.service.ts
```

3. `AuthService` отвечает за вход, регистрацию, подтверждение email, восстановление и сброс пароля, обновление access token, получение профиля и выход.

4. `CoursesService` отвечает за каталог, карточку курса, мои курсы, создание и редактирование курсов, публикацию, архивирование, запись на курс, модули и уроки.

5. `CourseBuilderService` отвечает за новое представление структуры курса через items: уроки, тесты, задания, live-занятия, ресурсы и внешние ссылки.

6. `ContentService` отвечает за блоки уроков: получение, создание, редактирование, удаление, перестановку и запуск кода.

7. `BlockAttemptsService` отвечает за отправку ответов по блокам, получение попыток, прогресса урока, истории запусков кода и ручную проверку.

8. DTO-типы используются для запросов и ответов. Они описаны интерфейсами TypeScript рядом с соответствующими сервисами.

9. Для форм и ответов сервера часто используются разные типы. Например для заданий есть `CreateAssignmentDto`, `AssignmentDto`, `AssignmentDetailDto`, `SubmissionDto`, `GradeSubmissionDto`.

10. Маппинг backend-данных в frontend-формат есть в `ContentService`: данные блоков нормализуются между `$type` и `type`, чтобы frontend мог работать с discriminated union типами.

## 7. Управление состоянием

1. Серверное состояние управляется через Angular services, RxJS Observables и signals внутри компонентов. TanStack Query или Redux Toolkit Query не используются.

2. Кэширование в виде отдельного query-cache слоя не реализовано. Данные обычно загружаются при открытии страницы и обновляются после мутаций.

3. Повторная загрузка после изменений выполняется вручную: например `CourseBuilderStore.refresh()`, `CourseEditorComponent.reloadModules()`, повторные вызовы сервисов после создания, удаления и редактирования.

4. Состояния загрузки отображаются через signals и boolean-флаги: `loading`, `saving`, `publishing`, `uploading`, `submitting`, `refreshing`.

5. Ошибки отображаются через `ToastService`, локальные сообщения компонентов и fallback-состояния в интерфейсе.

6. `queryKey`, `invalidateQueries`, `useMutation` не используются, так как TanStack Query отсутствует.

7. Локальное состояние интерфейса хранится в signals и обычных свойствах компонентов: открытие меню, модальных окон, выбранные фильтры, поисковые строки, вкладки, выбранные элементы, состояние мобильных панелей.

8. Глобальное состояние пользователя хранится в `AuthService`: `currentUser`, `accessToken`, `isAuthenticated`, `userRole`.

9. Состояние авторизации хранится в `AuthService` и синхронизируется с `localStorage`.

10. В `localStorage` хранятся `access_token`, `current_user`, а также признак показа onboarding для course builder: `cb-onboarding-shown`.

## 8. Авторизация и роли

1. Регистрация есть на маршруте `/register`.

2. Вход есть на маршруте `/login`.

3. Выход реализован через `AuthService.logout()`.

4. Роли: `Student`, `Teacher`, `Admin`.

5. Интерфейс отличается для разных ролей. Роль влияет на доступные маршруты, состав sidebar-меню, цветовой акцент sidebar, набор экранов и операции.

6. Защищенные маршруты есть для `/student`, `/teacher`, `/admin`, а также для ссылки на сообщения `/messages/:chatId`.

7. Frontend проверяет авторизацию через наличие `accessToken` и `currentUser` в `AuthService`. При обновлении страницы `ensureSessionRestored()` восстанавливает состояние из `localStorage`.

8. Access token и текущий пользователь хранятся в `localStorage`.

9. Если пользователь без прав открывает чужой раздел, `roleGuard` перенаправляет его в dashboard фактической роли или на `/login`.

10. Имя пользователя, инициалы и роль отображаются в `HeaderComponent`; профиль и настройки доступны через меню пользователя.

11. Восстановление пароля есть через `/forgot-password` и `/reset-password`. Изменение профиля и пароля реализовано в `ProfileComponent`.

## 9. Пользовательский интерфейс

1. Основные экраны реализованы для публичной зоны, студента, преподавателя и администратора.

2. На главной публичной странице размещен landing page платформы.

3. Dashboard есть для студента, преподавателя и администратора.

4. Табличные и списочные представления есть в администрировании пользователей, курсов, дисциплин, платежей, аналитики, журнале оценок, проверке работ, списке уведомлений, истории платежей.

5. Фильтрация есть в каталоге курсов, журнале оценок, админских списках, уведомлениях, словаре, сообщениях и отчетах.

6. Поиск используется в каталоге курсов, админском списке пользователей, списке курсов, словаре, сообщениях и компоненте выбора пользователя.

7. Сортировка в явном виде есть в каталоге курсов через параметр `sortBy`.

8. Пагинация реализована в каталоге курсов и админских списках через `page`, `pageSize`, `totalPages`.

9. Формы создания и редактирования есть для курсов, уроков, тестов, заданий, дисциплин, пользователей, профиля, расписания, словаря, платежных тарифов и настроек.

10. Модальные окна используются для подтверждений, onboarding course builder, выбора пользователя, публикации курса, создания/редактирования отдельных сущностей.

11. Подтверждение удаления реализовано через `confirm()` или отдельные inline-confirm блоки в интерфейсе.

12. Уведомления об успешных действиях и ошибках показываются через toast-систему.

13. Загрузчики реализованы через флаги `loading`, `uploading`, `saving`, `submitting`; в UI используются текстовые индикаторы, disabled-состояния и иконка `Loader2`.

14. Пустые состояния реализуются на страницах списков и карточек, например при отсутствии курсов, уведомлений, слов, сообщений, результатов.

15. Адаптивность есть: sidebar переходит в мобильный overlay, course builder имеет отдельные мобильные панели, layout использует media queries.

## 10. Формы и валидация

1. Формы есть для входа, регистрации, восстановления и сброса пароля, создания/редактирования курса, редактирования профиля, смены пароля, тестов, заданий, блоков урока, словаря, календарных событий, расписания, админских операций.

2. Формы реализованы через `ReactiveFormsModule` и `FormsModule`.

3. Клиентская валидация есть. Используются `Validators.required`, `Validators.email`, `Validators.minLength`, а также собственные проверки совпадения паролей.

4. Обязательные поля зависят от формы. Например в логине обязательны email и пароль; в регистрации обязательны имя, фамилия, email, пароль и подтверждение пароля; в создании курса обязательны название, описание, дисциплина и уровень.

5. Правила проверки включают корректный email, минимальную длину пароля 6 символов, минимальную длину имени/фамилии 2 символа, минимальную длину названия курса 5 символов, минимальную длину описания курса 20 символов, обязательность ключевых полей.

6. Сообщения об ошибках рядом с полями есть в auth-формах и формах курса.

7. Серверная валидация отображается через `ApiError` и toast-сообщения.

8. Переиспользуемые элементы формы есть: `InputComponent`, `ButtonComponent`, `RichTextEditorComponent`, `FileUploaderComponent`, `CourseSettingsFormComponent`.

9. Многошаговая форма есть в `CreateCourseComponent`: основные данные, настройки, публикация.

10. Есть rich text editor на Tiptap, загрузка файлов, выбор дат, drag-and-drop, Monaco code editor.

## 11. Компоненты

1. Основные переиспользуемые компоненты:

```text
ButtonComponent
InputComponent
CardComponent
BadgeComponent
AvatarComponent
CourseCardComponent
ProgressBarComponent
StatsCardComponent
SearchInputComponent
ToastComponent
FileUploaderComponent
FileCardComponent
RichTextEditorComponent
RichTextViewerComponent
VideoPlayerComponent
UserPickerComponent
```

2. Компоненты конкретных страниц находятся внутри `features`: например `CatalogComponent`, `CourseDetailComponent`, `LessonEditorComponent`, `TestPlayerComponent`, `AssignmentSubmitComponent`, `GradebookComponent`, `MessagesPageComponent`, `AdminUsersComponent`.

3. Общий layout есть: `AppLayoutComponent`.

4. Sidebar есть: `SidebarComponent`.

5. Header есть: `HeaderComponent`.

6. Универсального компонента таблицы не обнаружено, таблицы и списки реализованы на уровне конкретных страниц.

7. Карточка курса есть: `CourseCardComponent`.

8. Компонент загрузки файла есть: `FileUploaderComponent`.

9. Компонент текстового редактора есть: `RichTextEditorComponent`.

10. Обработка ошибок выполняется в компонентах через `ToastService`, локальные error-состояния и disabled-состояния кнопок.

## 12. Работа с файлами и медиа

1. Загрузка файлов реализована.

2. Поддерживаемые типы зависят от параметра `accept`. Общий uploader по умолчанию принимает `*/*`, а отдельные формы могут ограничивать типы, например обложки и аватары принимают изображения.

3. Загрузка выполняется через `FileService.upload(file, entityType, entityId)`, который отправляет `FormData` на `/api/files/upload`.

4. Drag-and-drop реализован в `FileUploaderComponent` и в загрузке обложки курса.

5. Предпросмотр изображений есть в `FileUploaderComponent` и при выборе обложки курса.

6. Удаление файлов реализовано через `FileService.deleteFile(id)` и `FileCardComponent`.

7. Прогресс загрузки в процентах не реализован, но есть состояние `uploading`.

8. Ограничения по размеру есть. В `FileUploaderComponent` значение по умолчанию - 100 MB. Для аватара и обложки курса используется ограничение 5 MB.

9. Ссылки на файлы хранятся в `AttachmentDto.fileUrl` или как `attachmentId` в блоках урока и элементах курса.

10. Пользователь прикрепляет материалы через файловый uploader в профиле, курсе, блоках урока, заданиях и сообщениях.

## 13. Редактор курса, модуля и урока

1. Отдельный редактор курса есть. Реализованы два связанных подхода: классический `CourseEditorComponent` и более функциональный `CourseBuilderComponent`.

2. Курс создается через `CreateCourseComponent`. Форма состоит из шагов: основные сведения, настройки, публикация. Поддерживаются черновик, публикация, обложка, дисциплина, уровень, платность, дедлайн, сертификат, порядок прохождения и оценивание.

3. Модуль создается через `CoursesService.createModule()` из редактора курса или course builder.

4. Урок создается через `CoursesService.createLesson()`.

5. В урок можно добавлять блоки контента:

```text
Text
Video
Audio
Image
Banner
File
SingleChoice
MultipleChoice
TrueFalse
FillGap
Dropdown
WordBank
Reorder
Matching
OpenText
CodeExercise
Quiz
Assignment
```

6. Порядок блоков в уроке можно менять через drag-and-drop на Angular CDK.

7. Порядок модулей и уроков также можно менять через drag-and-drop в дереве структуры курса.

8. Предпросмотр курса реализован через маршруты `/teacher/courses/:id/preview` и `/teacher/lesson-preview/:id`.

9. Черновики поддерживаются через статус курса и элементов. Курс можно сохранять как черновик до публикации.

10. Публикация курса реализована через `/api/courses/{id}/publish`; frontend показывает checklist с ошибками и предупреждениями публикации.

11. Статусы курса: опубликован, архивирован, черновик определяется через `isPublished` и `isArchived`. В course builder также есть статусы элементов: `Draft`, `NeedsContent`, `Ready`, `Published`, `Archived`.

12. Структура курса отображается как дерево разделов/модулей и элементов: уроки, тесты, задания, live-занятия, материалы и ссылки.

## 14. Тесты, задания и прогресс обучения

1. Тесты реализованы.

2. В моделях тестов вопрос имеет поле `type`, варианты ответов и баллы. В блочной системе урока дополнительно поддержаны интерактивные типы: один вариант, несколько вариантов, true/false, пропуски, dropdown, word bank, порядок, сопоставление, открытый ответ и кодовое упражнение.

3. Автоматическая проверка есть для закрытых и интерактивных блоков.

4. Ручная проверка преподавателем есть для открытых ответов, заданий, отдельных результатов тестов и code exercise review.

5. Задания реализованы.

6. Студент может сдавать текстовый ответ и прикреплять файлы через файловые компоненты.

7. Прогресс студента отображается через `ProgressService`, dashboard, страницу курса и урока.

8. Есть статус прохождения урока: урок можно отметить завершенным или снять отметку завершения.

9. Журнал оценок есть для преподавателя (`GradebookComponent`) и список оценок для студента (`StudentGradesComponent`).

10. Результаты тестирования показываются на странице `/student/test/:testId/result/:attemptId`.

## 15. Уведомления и обратная связь

1. Всплывающие уведомления есть.

2. Используется собственная toast-система: `ToastService` и `ToastComponent`.

3. Toast показывается при успешном входе, регистрации, сохранении курса, обновлении профиля, ошибках запросов, загрузке файлов, публикации и других действиях.

4. Подтверждения действий есть через `confirm()` и через специальные UI-блоки.

5. Сообщения об ошибках есть на уровне форм, toast и отдельных страниц.

6. Realtime-уведомления реализованы через SignalR: `/hubs/notifications` и `/hubs/chat`.

7. Индикаторы сохранения есть в редакторе курса, course builder и редакторе урока: `saving`, `saved`, `error`, `lastSaved`.

## 16. Адаптивность и UX

1. Приложение адаптировано под мобильные устройства.

2. На малых экранах sidebar превращается в выезжающее меню с overlay.

3. Боковое меню управляется `SidebarService`; для мобильной версии есть `mobileOpen`.

4. Media queries используются в SCSS, например breakpoint `1024px` для sidebar.

5. Tailwind не используется. Responsive-поведение реализовано собственными SCSS-стилями.

6. Темная тема как отдельный режим не реализована. Monaco editor использует темную тему внутри редактора кода.

7. Единый визуальный стиль задается через SCSS-переменные в `styles/_variables.scss`.

8. Основные цвета: slate-нейтральная палитра, indigo как основной акцент, amber для преподавателя, emerald для администратора, red для опасных действий, blue/teal/rose для дополнительных состояний. Шрифт: Inter с системными fallback.

9. Состояния hover/focus/disabled реализованы в компонентах и SCSS.

10. Доступность учитывается частично: используются кнопки, ссылки, `aria-label` у части icon-кнопок, disabled-состояния и визуальные focus/hover-состояния. Полноценная проверка accessibility отдельно не описана.

## 17. Обработка ошибок и надежность

1. Если backend недоступен, `parseApiError()` возвращает сообщение `Нет соединения с сервером.`.

2. При ошибке загрузки данных пользователь видит toast или локальное сообщение, а loading-состояние сбрасывается.

3. Angular global error listeners подключены через `provideBrowserGlobalErrorListeners()`. Отдельный React-style Error Boundary не применим, так как приложение Angular.

4. Отдельной страницы 404 нет, wildcard-маршрут перенаправляет на главную страницу.

5. Отдельной страницы 403 нет. При неверной роли `roleGuard` перенаправляет пользователя на dashboard его роли или login.

6. Fallback для изображений реализован локально в некоторых компонентах через аватар/инициалы, preview и проверки типа файла. Универсального глобального image fallback нет.

7. Повтор запроса есть для 401 после refresh token. Универсального retry для всех сетевых ошибок нет.

8. Защита от двойной отправки форм реализована через `loading`, `saving`, `submitting`, disabled-кнопки и проверки состояния.

9. Кнопки блокируются во время отправки в формах входа, регистрации, создания курса, теста, задания, оплаты и загрузки файлов.

10. Пустые данные проверяются через валидаторы форм, проверки `if (!id)`, `if (!file)`, `if (!code().trim())`, пустые состояния списков.

## 18. Сборка и запуск клиентской части

1. Папка фронтенда: `frontend`.

2. Установка зависимостей:

```bash
cd frontend
npm install
```

3. Локальный запуск:

```bash
cd frontend
npm start
```

По умолчанию приложение доступно на `http://localhost:4200`.

4. Production-сборка:

```bash
cd frontend
npm run build
```

5. Команда unit-тестов есть:

```bash
npm test
```

Команда lint в `package.json` не задана.

6. `.env` есть в корне проекта, а не внутри `frontend`. Для frontend важна переменная `BACKEND_URL` в dev proxy.

7. Переменные окружения из `.env.example`, связанные с фронтендом и локальным запуском: `BACKEND_URL`, `FRONTEND_URL`.

8. При локальном запуске frontend ходит на backend через Angular proxy: `/api`, `/hubs`, `/swagger` проксируются на `BACKEND_URL` или `http://localhost:5000`.

9. Dockerfile для frontend есть. Он собирает Angular-приложение в Node 22 Alpine и отдает production-бандл через nginx.

10. Для production nginx настроен так, чтобы SPA-маршруты возвращали `index.html`, а `/api`, `/hubs`, `/swagger` проксировались на backend.

## 19. Что лучше приложить кодом к диплому

Для приложения к пояснительной записке лучше всего вынести не весь код, а репрезентативные фрагменты:

```text
frontend/package.json
frontend/src/main.ts
frontend/src/app/app.config.ts
frontend/src/app/app.routes.ts
frontend/src/app/core/services/auth.service.ts
frontend/src/app/core/interceptors/auth.interceptor.ts
frontend/src/app/core/interceptors/error.interceptor.ts
frontend/src/app/core/guards/auth.guard.ts
frontend/src/app/core/guards/role.guard.ts
frontend/src/app/features/courses/services/courses.service.ts
frontend/src/app/features/content/services/content.service.ts
frontend/src/app/features/courses/course-builder/state/course-builder.store.ts
frontend/src/app/features/courses/lesson-editor/lesson-editor.component.ts
frontend/src/app/shared/components/file-uploader/file-uploader.component.ts
frontend/src/app/shared/components/rich-text-editor/rich-text-editor.component.ts
frontend/src/app/features/content/models/block-data.model.ts
```

Эти файлы показывают маршрутизацию, конфигурацию приложения, авторизацию, API-слой, управление состоянием, редактор урока, загрузку файлов и типизацию блочного контента.

## 20. Готовый текст для раздела 4.3

### 4.3 Программная реализация клиентской части

Клиентская часть программного средства EduPlatform реализована как отдельное одностраничное веб-приложение на Angular и TypeScript. Она размещена в каталоге `frontend` и взаимодействует с серверной частью через REST API и SignalR. Такой подход позволяет отделить пользовательский интерфейс от серверной бизнес-логики, упростить развитие отдельных экранов и обеспечить работу приложения без полной перезагрузки страницы при переходе между разделами.

Точкой входа клиентского приложения является файл `src/main.ts`, в котором выполняется bootstrap корневого компонента `App`. Конфигурация приложения вынесена в `app.config.ts`. В ней подключаются маршрутизатор Angular, HTTP-клиент, interceptors обработки ошибок и авторизации, а также конфигурация Monaco editor. Корневой компонент содержит основной `router-outlet` и глобальный компонент toast-уведомлений, благодаря чему всплывающие сообщения доступны во всех разделах приложения.

Структура frontend-проекта построена по модульному принципу. Каталог `core` содержит базовые сервисы, guards, interceptors и модели, которые используются во всем приложении. Каталог `shared` содержит переиспользуемые UI-компоненты, директивы и pipes. Каталог `layouts` содержит общие оболочки страниц: гостевой layout и layout авторизованной зоны. Каталог `features` разделяет приложение по функциональным областям: авторизация, курсы, контент, тесты, задания, оценки, прогресс, сообщения, уведомления, платежи, календарь, расписание, отчеты, инструменты и административная часть.

Маршрутизация реализована в файле `app.routes.ts`. Приложение разделено на публичную область и три защищенные области: кабинет студента, кабинет преподавателя и кабинет администратора. Публичная область включает главную страницу, каталог курсов, страницу курса, вход, регистрацию и восстановление пароля. Защищенные области используют `authGuard` для проверки факта авторизации и `roleGuard` для проверки роли пользователя. Для каждой роли определен собственный набор маршрутов и экранов.

Навигация авторизованной части построена на общем layout, включающем боковое меню, верхнюю панель и область основного содержимого. Боковое меню формируется динамически в `SidebarComponent` на основе роли текущего пользователя. Для студента отображаются дашборд, мои курсы, каталог, календарь, сообщения, уведомления, словарь и платежи. Для преподавателя дополнительно доступны создание курсов, проверка работ, журнал оценок, расписание, выплаты и отчеты. Для администратора доступны пользователи, курсы, дисциплины, платежи и аналитика. Активный пункт меню определяется средствами Angular Router через `RouterLinkActive`.

Взаимодействие с backend организовано через сервисный слой. Компоненты не формируют HTTP-запросы вручную, а используют специализированные сервисы: `AuthService`, `CoursesService`, `ContentService`, `TestsService`, `AssignmentsService`, `GradingService`, `ProgressService`, `PaymentsService`, `MessagingService`, `NotificationsService`, `AdminService` и другие. Такой подход снижает связность компонентов с API и позволяет хранить методы работы с сервером рядом с типами DTO соответствующего функционального модуля.

Базовый адрес API задается в файлах окружения как `/api`. В режиме разработки Angular dev server использует `proxy.conf.js`, который перенаправляет запросы `/api`, `/hubs` и `/swagger` на backend по адресу `http://localhost:5000` или по адресу из переменной окружения `BACKEND_URL`. В production-сборке аналогичное проксирование выполняется через nginx.

Авторизация реализована на основе JWT access token. После входа `AuthService` сохраняет access token и данные текущего пользователя в `localStorage`, а также загружает профиль через `/api/users/me`. Для защищенных запросов `authInterceptor` автоматически добавляет заголовок `Authorization: Bearer`. Если сервер возвращает 401, interceptor выполняет попытку обновления токена через `/api/auth/refresh` и повторяет исходный запрос. При невозможности обновить сессию пользователь выходит из системы и перенаправляется на страницу входа.

Обработка ошибок унифицирована через `errorInterceptor` и модель `ApiError`. Interceptor преобразует ошибки backend и стандартные HTTP-ошибки в единый объект с текстовым сообщением. Для сетевой ошибки выводится сообщение об отсутствии соединения с сервером, для 401 - сообщение об истечении сессии, для 403 - сообщение об отсутствии доступа, для 404 - сообщение о ненайденном ресурсе, для 500 - сообщение об ошибке сервера. На уровне интерфейса ошибки отображаются через toast-уведомления или локальные сообщения компонентов.

Состояние приложения управляется с помощью Angular signals, RxJS Observables и сервисов. Глобальное состояние авторизации хранится в `AuthService`, состояние бокового меню - в `SidebarService`, состояние toast-уведомлений - в `ToastService`, счетчики уведомлений и сообщений - в SignalR-сервисах. Серверные данные загружаются через сервисы при открытии страниц и обновляются после выполнения операций создания, редактирования или удаления. Для конструктора курса выделено локальное хранилище `CourseBuilderStore`, которое содержит состояние курса, выбранного элемента, признаков загрузки, автосохранения и готовности курса к публикации.

Одной из ключевых частей frontend является подсистема управления курсами. Преподаватель может создать курс через многошаговую форму, указать название, описание, дисциплину, уровень сложности, платность, дедлайн, сертификат и режим оценивания. После создания курс может редактироваться в структуре модулей и уроков либо в более развитом course builder. Course builder отображает курс как набор разделов и элементов: уроков, тестов, заданий, live-занятий, материалов и внешних ссылок. Для элементов курса поддерживаются статусы, обязательность, баллы, даты доступности и дедлайны.

Редактор урока реализован через блочную модель контента. Каждый урок состоит из набора блоков, порядок которых можно изменять drag-and-drop. Frontend поддерживает информационные блоки, интерактивные блоки с автоматической проверкой, блоки с ручной проверкой и составные блоки. К информационным блокам относятся текст, видео, аудио, изображение, баннер и файл. К автоматически проверяемым относятся одиночный выбор, множественный выбор, true/false, заполнение пропусков, dropdown, word bank, reorder и matching. К ручной проверке относятся открытый текст и упражнение с кодом. Составные блоки связывают урок с тестом или заданием.

Для редактирования содержимого используется `BlockEditorHostComponent`, который в зависимости от типа блока подключает соответствующий редактор. Для просмотра и прохождения блоков используется `BlockViewerHostComponent`. Данные блоков типизированы через discriminated union `LessonBlockData`, что позволяет frontend-коду обрабатывать разные виды контента с проверкой типов. При обмене с backend данные нормализуются в `ContentService`, где поле `$type` преобразуется в frontend-поле `type`.

Текстовый контент редактируется через `RichTextEditorComponent`, построенный на Tiptap. Редактор поддерживает форматирование текста, заголовки, списки, цитаты, ссылки, изображения, таблицы и code block. Для заданий по программированию используется Monaco editor. В блоке `CodeExercise` преподаватель задает язык, стартовый код, тест-кейсы, таймаут и скрытые проверки, а студент может запускать код, видеть результаты тест-кейсов, отправлять решение и просматривать историю запусков.

Файлы загружаются через общий `FileUploaderComponent`. Компонент поддерживает выбор файла, drag-and-drop, ограничение типа и размера, предпросмотр изображений, состояние загрузки и отправку файла через `FileService`. Файлы связываются с сущностями через `entityType` и `entityId`, что позволяет использовать один механизм для аватаров, обложек курсов, материалов уроков, заданий и сообщений.

Формы реализованы с использованием `ReactiveFormsModule` и `FormsModule`. В auth-формах применяются встроенные валидаторы email, обязательности и минимальной длины, а также пользовательская проверка совпадения паролей. В форме создания курса проверяются обязательность названия, описания, дисциплины и уровня, а также минимальная длина текстовых полей. Ошибки валидации отображаются рядом с полями, а ошибки backend - через toast-сообщения.

Клиентская часть также реализует подсистему тестирования и заданий. Студент может запустить попытку теста, отвечать на вопросы, завершить попытку и просмотреть результат. Преподаватель может создавать и редактировать тесты, добавлять вопросы, просматривать отправленные попытки и проверять ответы, требующие ручной оценки. Для заданий реализованы создание, редактирование, сдача студентом, просмотр отправок и выставление оценки преподавателем.

Прогресс обучения отображается через отдельный сервис `ProgressService`. Студент видит процент прохождения курса, статус уроков, оценки и ближайшие события. Преподаватель видит статистику по курсам, работы на проверку и отчеты. Журнал оценок реализован через `GradingService` и поддерживает просмотр успеваемости по курсу, статистику, а также экспорт в PDF и Excel.

Для коммуникации используются сообщения, уведомления и SignalR. `SignalRService` подключается к `/hubs/notifications` и обновляет счетчик непрочитанных уведомлений, а `ChatSignalRService` подключается к `/hubs/chat` и обрабатывает новые сообщения, редактирование, удаление, прочтение и изменения участников чата. Header и sidebar используют эти счетчики для отображения актуального состояния без ручного обновления страницы.

Административная часть реализована отдельным набором страниц и сервисом `AdminService`. Администратор может просматривать и фильтровать пользователей, создавать пользователя, блокировать и разблокировать аккаунты, менять роли, удалять пользователей, просматривать и архивировать курсы, управлять дисциплинами, анализировать платежи, создавать возвраты, управлять подписочными планами, просматривать аналитические показатели и изменять настройки платформы.

Визуальный стиль приложения задается SCSS-переменными в `styles/_variables.scss`. В проекте используется светлая тема, нейтральная slate-палитра и ролевые акценты: indigo для студента, amber для преподавателя и emerald для администратора. Интерфейс адаптивен: на ширине меньше 1024 px боковое меню становится выезжающей мобильной панелью с затемнением фона, а сложные экраны, такие как course builder, используют отдельные мобильные панели.

Сборка и запуск клиентской части выполняются средствами Angular CLI. Для локальной разработки используются команды `npm install` и `npm start` в каталоге `frontend`; приложение запускается на `http://localhost:4200`. Production-сборка выполняется командой `npm run build`. Для контейнерного запуска предусмотрен `Dockerfile`, который собирает Angular-приложение и размещает результат в nginx. Конфигурация nginx обеспечивает корректную работу SPA-маршрутов и проксирование API, SignalR и Swagger-запросов на backend.

Таким образом, клиентская часть EduPlatform представляет собой модульное Angular-приложение с ролевой маршрутизацией, сервисным слоем для взаимодействия с backend, централизованной обработкой авторизации и ошибок, развитой системой компонентов и поддержкой сложных образовательных сценариев. Реализованный frontend обеспечивает работу всех основных ролей системы и связывает пользовательский интерфейс с серверными модулями курсов, контента, тестирования, заданий, оценивания, платежей, уведомлений и администрирования.

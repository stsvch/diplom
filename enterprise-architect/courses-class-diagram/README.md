# Courses Class Diagram для Enterprise Architect

Папка содержит комплект файлов для диаграммы классов модуля `Courses`.

## Файлы

| Файл | Для чего нужен |
|---|---|
| `courses-class-diagram.xmi` | Черновик UML-модели для импорта в Enterprise Architect через XMI. |
| `courses-class-diagram.puml` | PlantUML-версия диаграммы. Удобна для быстрой проверки и ручного переноса в EA. |
| `courses-class-diagram.mmd` | Mermaid-версия как запасной визуальный формат. |
| `ea-import-notes.md` | Инструкция: как импортировать/перенести диаграмму в Enterprise Architect. |

## Важное ограничение

Полноценный файл проекта Enterprise Architect (`.qea`, `.qeax`, `.eap`, `.eapx`) надёжно создать без самого Enterprise Architect нельзя. Эти форматы являются проектными базами EA, и корректнее всего они создаются самим приложением.

Обходной путь:

1. Импортировать `courses-class-diagram.xmi` в EA.
2. Если XMI импортируется неидеально, использовать `courses-class-diagram.puml` и `ea-import-notes.md` для ручного создания диаграммы.
3. Если нужен только рисунок для диплома, сгенерировать PNG/SVG из PlantUML или Mermaid.

## Что отражает диаграмма

Диаграмма построена по фактическому коду модуля:

- `backend/src/Modules/Courses/Courses.Domain/Entities`
- `backend/src/Modules/Courses/Courses.Domain/Enums`
- `backend/src/Modules/Courses/Courses.Infrastructure/Persistence/CoursesDbContext.cs`

Основные классы:

- `Discipline`
- `Course`
- `CourseModule`
- `Lesson`
- `CourseItem`
- `CourseEnrollment`
- `CourseReview`

Основные enum-ы:

- `CourseLevel`
- `CourseOrderType`
- `EnrollmentStatus`
- `CourseItemType`
- `CourseItemStatus`
- `LessonLayout`


# Инструкция для Enterprise Architect

## Вариант 1. Попробовать импорт XMI

1. Открыть Enterprise Architect.
2. Создать новый project/package, например `EduPlatform`.
3. Создать package `Courses Module`.
4. Выбрать package `Courses Module`.
5. Выполнить импорт XMI:
   - `Publish` / `Import/Export`
   - `Import Package from XMI`
   - выбрать `courses-class-diagram.xmi`
6. После импорта создать `Class Diagram`.
7. Перетащить импортированные классы на диаграмму.
8. Если связи не появились автоматически, выбрать `Insert Related Elements` или добавить связи вручную по таблице ниже.

## Вариант 2. Ручной перенос, если XMI не подошёл

Создать классы:

- `BaseEntity`
- `IAuditableEntity`
- `Discipline`
- `Course`
- `CourseModule`
- `Lesson`
- `CourseItem`
- `CourseEnrollment`
- `CourseReview`

Создать enum-ы:

- `CourseLevel`
- `CourseOrderType`
- `EnrollmentStatus`
- `CourseItemType`
- `CourseItemStatus`
- `LessonLayout`

## Связи

| Source | Target | Relation | Multiplicity |
|---|---|---|---|
| `Discipline` | `BaseEntity` | Generalization | - |
| `Course` | `BaseEntity` | Generalization | - |
| `CourseModule` | `BaseEntity` | Generalization | - |
| `Lesson` | `BaseEntity` | Generalization | - |
| `CourseItem` | `BaseEntity` | Generalization | - |
| `CourseEnrollment` | `BaseEntity` | Generalization | - |
| `CourseReview` | `BaseEntity` | Generalization | - |
| `Discipline` | `IAuditableEntity` | Realization | - |
| `Course` | `IAuditableEntity` | Realization | - |
| `CourseItem` | `IAuditableEntity` | Realization | - |
| `CourseReview` | `IAuditableEntity` | Realization | - |
| `Discipline` | `Course` | Aggregation/Association | `1` to `0..*` |
| `Course` | `CourseModule` | Composition | `1` to `0..*` |
| `Course` | `CourseItem` | Composition | `1` to `0..*` |
| `Course` | `CourseEnrollment` | Composition | `1` to `0..*` |
| `Course` | `CourseReview` | Composition | `1` to `0..*` |
| `CourseModule` | `Lesson` | Composition | `1` to `0..*` |
| `CourseModule` | `CourseItem` | Aggregation/Association | `0..1` to `0..*` |

## Примечание для CourseItem

Добавить note рядом с `CourseItem`:

```text
CourseItem is a universal Course Builder item.
Type + SourceId identify source entity.
Source can be Lesson, Test, Assignment, LiveSession, Resource or ExternalLink.
Unique index: { Type, SourceId }.
```

Не нужно проводить прямые association от `CourseItem` ко всем внешним сущностям `Test`, `Assignment`, `ScheduleSlot`. В коде `SourceId` не является жёстким FK. Это полиморфная ссылка, определяемая через `CourseItemType`.

## Рекомендуемая раскладка

```text
Discipline 1 ---- 0..* Course
                       |
                       | 1 ---- 0..* CourseModule
                       |              |
                       |              | 1 ---- 0..* Lesson
                       |              |
                       |              | 0..1 ---- 0..* CourseItem
                       |
                       | 1 ---- 0..* CourseItem
                       |
                       | 1 ---- 0..* CourseEnrollment
                       |
                       | 1 ---- 0..* CourseReview
```


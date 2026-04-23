# Логическая модель базы данных EduPlatform

## Обзор

Система использует **PostgreSQL** (реляционные данные, 10 схем) и **MongoDB** (чаты и сообщения).
Файлы хранятся в **S3/MinIO**.

Общее количество сущностей: **27 в PostgreSQL**, **2 коллекции в MongoDB**.

---

## Схема: auth

### ApplicationUser
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | string | PK |
| Email | string | UNIQUE, NOT NULL |
| PasswordHash | string | NOT NULL |
| FirstName | string | NOT NULL |
| LastName | string | NOT NULL |
| Role | enum | NOT NULL (Admin / Teacher / Student) |
| AvatarUrl | string | NULL |
| IsActive | boolean | NOT NULL, DEFAULT true |
| EmailConfirmed | boolean | NOT NULL, DEFAULT false |
| CreatedAt | DateTime | NOT NULL |
| UpdatedAt | DateTime | NULL |

### RefreshToken
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| Token | string | NOT NULL |
| UserId | string | FK → ApplicationUser(Id), NOT NULL |
| ExpiresAt | DateTime | NOT NULL |
| CreatedAt | DateTime | NOT NULL |
| IsRevoked | boolean | NOT NULL, DEFAULT false |

**Связи:**
- ApplicationUser (1) → (0..*) RefreshToken

---

## Схема: courses

### Discipline
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| Name | string(100) | NOT NULL, UNIQUE |
| Description | string | NULL |
| ImageUrl | string | NULL |
| CreatedAt | DateTime | NOT NULL |

### Course
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| DisciplineId | GUID | FK → Discipline(Id), NOT NULL |
| TeacherId | string | FK → ApplicationUser(Id), NOT NULL |
| TeacherName | string | NOT NULL |
| Title | string(200) | NOT NULL |
| Description | string | NOT NULL |
| Price | decimal | NULL |
| IsFree | boolean | NOT NULL |
| IsPublished | boolean | NOT NULL, DEFAULT false |
| IsArchived | boolean | NOT NULL, DEFAULT false |
| OrderType | enum | NOT NULL (Sequential / Free) |
| HasGrading | boolean | NOT NULL, DEFAULT true |
| Level | enum | NOT NULL (Beginner / Intermediate / Advanced) |
| ImageUrl | string | NULL |
| Tags | string | NULL |
| CreatedAt | DateTime | NOT NULL |
| UpdatedAt | DateTime | NULL |

### CourseEnrollment
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| CourseId | GUID | FK → Course(Id), NOT NULL |
| StudentId | string | FK → ApplicationUser(Id), NOT NULL |
| EnrolledAt | DateTime | NOT NULL |
| Status | enum | NOT NULL (Active / Completed / Dropped) |

**Индекс:** UNIQUE (CourseId, StudentId)

### CourseModule
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| CourseId | GUID | FK → Course(Id), NOT NULL, CASCADE DELETE |
| Title | string(200) | NOT NULL |
| Description | string | NULL |
| OrderIndex | int | NOT NULL |
| IsPublished | boolean | NOT NULL, DEFAULT false |

### Lesson
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| ModuleId | GUID | FK → CourseModule(Id), NOT NULL, CASCADE DELETE |
| Title | string(200) | NOT NULL |
| Description | string | NULL |
| OrderIndex | int | NOT NULL |
| IsPublished | boolean | NOT NULL, DEFAULT false |
| Duration | int | NULL (минуты) |

### LessonBlock
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| LessonId | GUID | FK → Lesson(Id), NOT NULL, CASCADE DELETE |
| OrderIndex | int | NOT NULL |
| Type | enum | NOT NULL (Text / Video / File / Quiz / Assignment / Exercise) |
| TextContent | string | NULL |
| VideoUrl | string | NULL |
| TestId | GUID | NULL (логическая связь с Test) |
| AssignmentId | GUID | NULL (логическая связь с Assignment) |

**Связи схемы courses:**
```
Discipline (1) ──── (0..*) Course
ApplicationUser (1) ──── (0..*) Course [TeacherId]
Course (1) ──── (0..*) CourseEnrollment
ApplicationUser (1) ──── (0..*) CourseEnrollment [StudentId]
Course (1) ──── (0..*) CourseModule
CourseModule (1) ──── (0..*) Lesson
Lesson (1) ──── (0..*) LessonBlock
LessonBlock (0..1) ···· (0..1) Test [TestId, без FK constraint]
LessonBlock (0..1) ···· (0..1) Assignment [AssignmentId, без FK constraint]
```

---

## Схема: content

### Attachment
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| FileName | string | NOT NULL |
| StoragePath | string | NOT NULL |
| FileUrl | string | NOT NULL |
| ContentType | string | NOT NULL |
| FileSize | long | NOT NULL |
| EntityType | enum | NOT NULL (LessonBlock / Assignment / AssignmentSubmission / Comment / Exercise / DictionaryWord / UserAvatar) |
| EntityId | GUID | NOT NULL |
| UploadedById | string | FK → ApplicationUser(Id), NOT NULL |
| CreatedAt | DateTime | NOT NULL |

**Индекс:** (EntityType, EntityId)

**Связь:** полиморфная — привязывается к любой сущности через EntityType + EntityId

---

## Схема: tests

### Test
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| Title | string(200) | NOT NULL |
| Description | string | NULL |
| CreatedById | string | FK → ApplicationUser(Id), NOT NULL |
| TimeLimitMinutes | int | NULL |
| MaxAttempts | int | NULL |
| Deadline | DateTime | NULL |
| ShuffleQuestions | boolean | NOT NULL, DEFAULT false |
| ShuffleAnswers | boolean | NOT NULL, DEFAULT false |
| ShowCorrectAnswers | boolean | NOT NULL, DEFAULT true |
| MaxScore | int | NOT NULL |
| CreatedAt | DateTime | NOT NULL |
| UpdatedAt | DateTime | NULL |

### Question
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| TestId | GUID | FK → Test(Id), NOT NULL, CASCADE DELETE |
| Type | enum | NOT NULL (SingleChoice / MultipleChoice / TextInput / Matching / OpenAnswer) |
| Text | string | NOT NULL |
| Points | int | NOT NULL |
| OrderIndex | int | NOT NULL |

### AnswerOption
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| QuestionId | GUID | FK → Question(Id), NOT NULL, CASCADE DELETE |
| Text | string | NOT NULL |
| IsCorrect | boolean | NOT NULL |
| OrderIndex | int | NOT NULL |
| MatchingPairValue | string | NULL |

### TestAttempt
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| TestId | GUID | FK → Test(Id), NOT NULL |
| StudentId | string | FK → ApplicationUser(Id), NOT NULL |
| AttemptNumber | int | NOT NULL |
| StartedAt | DateTime | NOT NULL |
| CompletedAt | DateTime | NULL |
| Score | int | NULL |
| Status | enum | NOT NULL (InProgress / Completed / NeedsReview) |

### TestResponse
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| AttemptId | GUID | FK → TestAttempt(Id), NOT NULL, CASCADE DELETE |
| QuestionId | GUID | FK → Question(Id), NOT NULL |
| SelectedOptionIds | string | NULL (JSON) |
| TextAnswer | string | NULL |
| IsCorrect | boolean | NULL |
| Points | int | NULL |
| TeacherComment | string | NULL |

**Индекс:** UNIQUE (AttemptId, QuestionId)

**Связи схемы tests:**
```
Test (1) ──── (0..*) Question
Question (1) ──── (0..*) AnswerOption
Test (1) ──── (0..*) TestAttempt
TestAttempt (1) ──── (0..*) TestResponse
TestResponse (0..*) ──── (1) Question
```

---

## Схема: assignments

### Assignment
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| Title | string(200) | NOT NULL |
| Description | string | NOT NULL |
| Criteria | string | NULL |
| Deadline | DateTime | NULL |
| MaxAttempts | int | NULL |
| MaxScore | int | NOT NULL |
| CreatedById | string | FK → ApplicationUser(Id), NOT NULL |
| CreatedAt | DateTime | NOT NULL |
| UpdatedAt | DateTime | NULL |

### AssignmentSubmission
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| AssignmentId | GUID | FK → Assignment(Id), NOT NULL, CASCADE DELETE |
| StudentId | string | FK → ApplicationUser(Id), NOT NULL |
| AttemptNumber | int | NOT NULL |
| Content | string | NULL |
| SubmittedAt | DateTime | NOT NULL |
| Status | enum | NOT NULL (Submitted / UnderReview / Graded / ReturnedForRevision) |
| Score | int | NULL |
| TeacherComment | string | NULL |
| GradedAt | DateTime | NULL |
| GradedById | string | NULL |

**Индекс:** UNIQUE (AssignmentId, StudentId, AttemptNumber)

**Связи:**
```
Assignment (1) ──── (0..*) AssignmentSubmission
```

---

## Схема: grading

### Grade
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| StudentId | string | FK → ApplicationUser(Id), NOT NULL |
| CourseId | GUID | NOT NULL |
| SourceType | enum | NOT NULL (Test / Assignment) |
| TestAttemptId | GUID | NULL |
| AssignmentSubmissionId | GUID | NULL |
| Title | string | NOT NULL |
| Score | decimal | NOT NULL |
| MaxScore | decimal | NOT NULL |
| Comment | string | NULL |
| GradedAt | DateTime | NOT NULL |
| GradedById | string | NULL |
| CreatedAt | DateTime | NOT NULL |
| UpdatedAt | DateTime | NULL |

**Индексы:** (StudentId, CourseId), (CourseId)

---

## Схема: progress

### LessonProgress
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| LessonId | GUID | NOT NULL |
| StudentId | string | FK → ApplicationUser(Id), NOT NULL |
| IsCompleted | boolean | NOT NULL, DEFAULT false |
| CompletedAt | DateTime | NULL |

**Индекс:** UNIQUE (LessonId, StudentId)

---

## Схема: notifications

### Notification
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| UserId | string | FK → ApplicationUser(Id), NOT NULL |
| Type | enum | NOT NULL (Grade / Deadline / Message / Course / Achievement) |
| Title | string | NOT NULL |
| Message | string | NOT NULL |
| IsRead | boolean | NOT NULL, DEFAULT false |
| LinkUrl | string | NULL |
| CreatedAt | DateTime | NOT NULL |

**Индексы:** (UserId, IsRead), (UserId, CreatedAt DESC)

---

## Схема: calendar

### CalendarEvent
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| UserId | string | NULL |
| CourseId | GUID | NULL |
| Title | string | NOT NULL |
| Description | string | NULL |
| EventDate | DateTime | NOT NULL |
| EventTime | string | NULL |
| Type | enum | NOT NULL (Deadline / Lesson / Quiz / Workshop / Custom) |
| SourceType | string | NULL |
| SourceId | GUID | NULL |
| CreatedAt | DateTime | NOT NULL |

**Индексы:** (UserId, EventDate), (EventDate)

---

## Схема: scheduling

### ScheduleSlot
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| TeacherId | string | FK → ApplicationUser(Id), NOT NULL |
| TeacherName | string | NOT NULL |
| CourseId | GUID | NULL |
| CourseName | string | NULL |
| Title | string | NOT NULL |
| Description | string | NULL |
| StartTime | DateTime | NOT NULL |
| EndTime | DateTime | NOT NULL |
| IsGroupSession | boolean | NOT NULL |
| MaxStudents | int | NOT NULL, DEFAULT 1 |
| Status | enum | NOT NULL (Available / Booked / Completed / Cancelled) |
| MeetingLink | string | NULL |
| CreatedAt | DateTime | NOT NULL |
| UpdatedAt | DateTime | NULL |

### SessionBooking
| Поле | Тип | Ограничения |
|------|-----|-------------|
| Id | GUID | PK |
| SlotId | GUID | FK → ScheduleSlot(Id), NOT NULL, CASCADE DELETE |
| StudentId | string | FK → ApplicationUser(Id), NOT NULL |
| StudentName | string | NOT NULL |
| BookedAt | DateTime | NOT NULL |
| Status | enum | NOT NULL (Booked / Completed / Cancelled) |

**Индекс:** UNIQUE (SlotId, StudentId)

**Связи:**
```
ScheduleSlot (1) ──── (0..*) SessionBooking
ApplicationUser (1) ──── (0..*) ScheduleSlot [TeacherId]
ApplicationUser (1) ──── (0..*) SessionBooking [StudentId]
```

---

## MongoDB: messaging

### Коллекция: chats
| Поле | Тип | Описание |
|------|-----|----------|
| _id | ObjectId | PK |
| Type | string | "DirectMessage" / "CourseChat" |
| CourseId | string | NULL, для курсовых чатов |
| CourseName | string | NULL |
| ParticipantIds | string[] | ID участников |
| Participants | ParticipantInfo[] | {UserId, Name} |
| LastMessage | string | NULL |
| LastMessageAt | DateTime | NULL |
| CreatedAt | DateTime | |

**Индексы:** ParticipantIds (multikey)

### Коллекция: messages
| Поле | Тип | Описание |
|------|-----|----------|
| _id | ObjectId | PK |
| ChatId | string | FK → chats._id |
| SenderId | string | ID отправителя |
| SenderName | string | Имя отправителя |
| Text | string | Текст сообщения |
| Attachments | MessageAttachment[] | {FileName, FileUrl, ContentType, FileSize} |
| SentAt | DateTime | |
| ReadBy | string[] | ID прочитавших |
| IsEdited | boolean | |

**Индексы:** ChatId, (ChatId, SentAt DESC)

---

## Общая карта связей

```
                    ┌──────────────┐
                    │ APPLICATION  │
                    │    USER      │
                    └──────┬───────┘
           ┌───────┬───────┼───────┬────────┬──────────┐
           │       │       │       │        │          │
           ▼       ▼       ▼       ▼        ▼          ▼
      ┌────────┐┌──────┐┌─────┐┌──────┐┌────────┐┌──────────┐
      │Refresh ││Course││Test ││Assign││Schedule││Notifica- │
      │Token   ││      ││     ││ment  ││Slot    ││tion      │
      └────────┘└──┬───┘└──┬──┘└──┬───┘└───┬────┘└──────────┘
                   │       │      │        │
           ┌───────┤       │      │        │
           │       │       │      │        ▼
     ┌─────┴──┐┌───┴────┐  │      │   ┌─────────┐
     │Discipl-││Course  │  │      │   │Session  │
     │ine     ││Enroll- │  │      │   │Booking  │
     └────────┘│ment    │  │      │   └─────────┘
               └────────┘  │      │
                   │       │      ▼
              ┌────┴───┐   │ ┌─────────┐
              │Course  │   │ │Assign.  │
              │Module  │   │ │Submis-  │
              └───┬────┘   │ │sion     │
                  │        │ └─────────┘
              ┌───┴───┐    │
              │Lesson │    │       ┌───────┐
              └───┬───┘    │       │Grade  │
                  │        │       └───────┘
            ┌─────┴─────┐  │
            │Lesson     │  │       ┌────────────┐
            │Block      │··│·······│Attachment   │
            └───────────┘  │       │(полиморф.) │
                           │       └────────────┘
                    ┌──────┴──┐
                    │Question │    ┌──────────────┐
                    └────┬────┘    │Lesson       │
                         │        │Progress     │
                    ┌────┴─────┐  └──────────────┘
                    │Answer    │
                    │Option    │  ┌──────────────┐
                    └──────────┘  │Calendar     │
                                  │Event        │
               ┌──────────┐      └──────────────┘
               │Test      │
               │Attempt   │
               └────┬─────┘
                    │
               ┌────┴─────┐
               │Test      │
               │Response  │
               └──────────┘
```

---

## Для визуализации

Скопируй код ниже в [dbdiagram.io](https://dbdiagram.io) для автоматической генерации визуальной диаграммы:

```dbml
// === AUTH ===
Table ApplicationUser {
  Id varchar [pk]
  Email varchar [unique, not null]
  PasswordHash varchar [not null]
  FirstName varchar [not null]
  LastName varchar [not null]
  Role varchar [not null, note: 'Admin/Teacher/Student']
  AvatarUrl varchar
  IsActive boolean [not null, default: true]
  EmailConfirmed boolean [not null, default: false]
  CreatedAt datetime [not null]
  UpdatedAt datetime
}

Table RefreshToken {
  Id uuid [pk]
  Token varchar [not null]
  UserId varchar [not null, ref: > ApplicationUser.Id]
  ExpiresAt datetime [not null]
  CreatedAt datetime [not null]
  IsRevoked boolean [not null, default: false]
}

// === COURSES ===
Table Discipline {
  Id uuid [pk]
  Name varchar [unique, not null]
  Description varchar
  ImageUrl varchar
  CreatedAt datetime [not null]
}

Table Course {
  Id uuid [pk]
  DisciplineId uuid [not null, ref: > Discipline.Id]
  TeacherId varchar [not null, ref: > ApplicationUser.Id]
  TeacherName varchar [not null]
  Title varchar [not null]
  Description varchar [not null]
  Price decimal
  IsFree boolean [not null]
  IsPublished boolean [not null, default: false]
  IsArchived boolean [not null, default: false]
  OrderType varchar [not null, note: 'Sequential/Free']
  HasGrading boolean [not null, default: true]
  Level varchar [not null, note: 'Beginner/Intermediate/Advanced']
  ImageUrl varchar
  Tags varchar
  CreatedAt datetime [not null]
  UpdatedAt datetime
}

Table CourseEnrollment {
  Id uuid [pk]
  CourseId uuid [not null, ref: > Course.Id]
  StudentId varchar [not null, ref: > ApplicationUser.Id]
  EnrolledAt datetime [not null]
  Status varchar [not null, note: 'Active/Completed/Dropped']

  indexes {
    (CourseId, StudentId) [unique]
  }
}

Table CourseModule {
  Id uuid [pk]
  CourseId uuid [not null, ref: > Course.Id]
  Title varchar [not null]
  Description varchar
  OrderIndex int [not null]
  IsPublished boolean [not null, default: false]
}

Table Lesson {
  Id uuid [pk]
  ModuleId uuid [not null, ref: > CourseModule.Id]
  Title varchar [not null]
  Description varchar
  OrderIndex int [not null]
  IsPublished boolean [not null, default: false]
  Duration int [note: 'minutes']
}

Table LessonBlock {
  Id uuid [pk]
  LessonId uuid [not null, ref: > Lesson.Id]
  OrderIndex int [not null]
  Type varchar [not null, note: 'Text/Video/File/Quiz/Assignment/Exercise']
  TextContent text
  VideoUrl varchar
  TestId uuid [note: 'logical ref to Test']
  AssignmentId uuid [note: 'logical ref to Assignment']
}

// === CONTENT ===
Table Attachment {
  Id uuid [pk]
  FileName varchar [not null]
  StoragePath varchar [not null]
  FileUrl varchar [not null]
  ContentType varchar [not null]
  FileSize bigint [not null]
  EntityType varchar [not null]
  EntityId uuid [not null]
  UploadedById varchar [not null, ref: > ApplicationUser.Id]
  CreatedAt datetime [not null]

  indexes {
    (EntityType, EntityId)
  }
}

// === TESTS ===
Table Test {
  Id uuid [pk]
  Title varchar [not null]
  Description varchar
  CreatedById varchar [not null, ref: > ApplicationUser.Id]
  TimeLimitMinutes int
  MaxAttempts int
  Deadline datetime
  ShuffleQuestions boolean [not null, default: false]
  ShuffleAnswers boolean [not null, default: false]
  ShowCorrectAnswers boolean [not null, default: true]
  MaxScore int [not null]
  CreatedAt datetime [not null]
  UpdatedAt datetime
}

Table Question {
  Id uuid [pk]
  TestId uuid [not null, ref: > Test.Id]
  Type varchar [not null, note: 'SingleChoice/MultipleChoice/TextInput/Matching/OpenAnswer']
  Text text [not null]
  Points int [not null]
  OrderIndex int [not null]
}

Table AnswerOption {
  Id uuid [pk]
  QuestionId uuid [not null, ref: > Question.Id]
  Text varchar [not null]
  IsCorrect boolean [not null]
  OrderIndex int [not null]
  MatchingPairValue varchar
}

Table TestAttempt {
  Id uuid [pk]
  TestId uuid [not null, ref: > Test.Id]
  StudentId varchar [not null, ref: > ApplicationUser.Id]
  AttemptNumber int [not null]
  StartedAt datetime [not null]
  CompletedAt datetime
  Score int
  Status varchar [not null, note: 'InProgress/Completed/NeedsReview']
}

Table TestResponse {
  Id uuid [pk]
  AttemptId uuid [not null, ref: > TestAttempt.Id]
  QuestionId uuid [not null, ref: > Question.Id]
  SelectedOptionIds varchar [note: 'JSON array']
  TextAnswer text
  IsCorrect boolean
  Points int
  TeacherComment text

  indexes {
    (AttemptId, QuestionId) [unique]
  }
}

// === ASSIGNMENTS ===
Table Assignment {
  Id uuid [pk]
  Title varchar [not null]
  Description text [not null]
  Criteria text
  Deadline datetime
  MaxAttempts int
  MaxScore int [not null]
  CreatedById varchar [not null, ref: > ApplicationUser.Id]
  CreatedAt datetime [not null]
  UpdatedAt datetime
}

Table AssignmentSubmission {
  Id uuid [pk]
  AssignmentId uuid [not null, ref: > Assignment.Id]
  StudentId varchar [not null, ref: > ApplicationUser.Id]
  AttemptNumber int [not null]
  Content text
  SubmittedAt datetime [not null]
  Status varchar [not null, note: 'Submitted/UnderReview/Graded/ReturnedForRevision']
  Score int
  TeacherComment text
  GradedAt datetime
  GradedById varchar

  indexes {
    (AssignmentId, StudentId, AttemptNumber) [unique]
  }
}

// === GRADING ===
Table Grade {
  Id uuid [pk]
  StudentId varchar [not null, ref: > ApplicationUser.Id]
  CourseId uuid [not null]
  SourceType varchar [not null, note: 'Test/Assignment']
  TestAttemptId uuid
  AssignmentSubmissionId uuid
  Title varchar [not null]
  Score decimal [not null]
  MaxScore decimal [not null]
  Comment text
  GradedAt datetime [not null]
  GradedById varchar
  CreatedAt datetime [not null]
  UpdatedAt datetime
}

// === PROGRESS ===
Table LessonProgress {
  Id uuid [pk]
  LessonId uuid [not null]
  StudentId varchar [not null, ref: > ApplicationUser.Id]
  IsCompleted boolean [not null, default: false]
  CompletedAt datetime

  indexes {
    (LessonId, StudentId) [unique]
  }
}

// === NOTIFICATIONS ===
Table Notification {
  Id uuid [pk]
  UserId varchar [not null, ref: > ApplicationUser.Id]
  Type varchar [not null, note: 'Grade/Deadline/Message/Course/Achievement']
  Title varchar [not null]
  Message text [not null]
  IsRead boolean [not null, default: false]
  LinkUrl varchar
  CreatedAt datetime [not null]
}

// === CALENDAR ===
Table CalendarEvent {
  Id uuid [pk]
  UserId varchar
  CourseId uuid
  Title varchar [not null]
  Description text
  EventDate datetime [not null]
  EventTime varchar
  Type varchar [not null, note: 'Deadline/Lesson/Quiz/Workshop/Custom']
  SourceType varchar
  SourceId uuid
  CreatedAt datetime [not null]
}

// === SCHEDULING ===
Table ScheduleSlot {
  Id uuid [pk]
  TeacherId varchar [not null, ref: > ApplicationUser.Id]
  TeacherName varchar [not null]
  CourseId uuid
  CourseName varchar
  Title varchar [not null]
  Description text
  StartTime datetime [not null]
  EndTime datetime [not null]
  IsGroupSession boolean [not null]
  MaxStudents int [not null, default: 1]
  Status varchar [not null, note: 'Available/Booked/Completed/Cancelled']
  MeetingLink varchar
  CreatedAt datetime [not null]
  UpdatedAt datetime
}

Table SessionBooking {
  Id uuid [pk]
  SlotId uuid [not null, ref: > ScheduleSlot.Id]
  StudentId varchar [not null, ref: > ApplicationUser.Id]
  StudentName varchar [not null]
  BookedAt datetime [not null]
  Status varchar [not null, note: 'Booked/Completed/Cancelled']

  indexes {
    (SlotId, StudentId) [unique]
  }
}
```

-- ============================================================
-- Логическая модель БД EduPlatform
-- Для импорта в Enterprise Architect:
-- Tools → Database Engineering → Import DDL Script
-- DBMS: PostgreSQL
-- ============================================================

-- ======================== SCHEMA: auth ========================

CREATE TABLE ApplicationUser (
    Id VARCHAR(450) NOT NULL,
    Email VARCHAR(256) NOT NULL,
    PasswordHash VARCHAR(500) NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    AvatarUrl VARCHAR(500),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    EmailConfirmed BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT PK_ApplicationUser PRIMARY KEY (Id),
    CONSTRAINT UQ_ApplicationUser_Email UNIQUE (Email)
);

CREATE TABLE RefreshToken (
    Id UUID NOT NULL,
    Token VARCHAR(500) NOT NULL,
    UserId VARCHAR(450) NOT NULL,
    ExpiresAt TIMESTAMP NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    IsRevoked BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT PK_RefreshToken PRIMARY KEY (Id),
    CONSTRAINT UQ_RefreshToken_Token UNIQUE (Token),
    CONSTRAINT FK_RefreshToken_User FOREIGN KEY (UserId) REFERENCES ApplicationUser(Id) ON DELETE CASCADE
);

-- ======================== SCHEMA: courses ========================

CREATE TABLE Discipline (
    Id UUID NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Description TEXT,
    ImageUrl VARCHAR(500),
    CreatedAt TIMESTAMP NOT NULL,
    CONSTRAINT PK_Discipline PRIMARY KEY (Id),
    CONSTRAINT UQ_Discipline_Name UNIQUE (Name)
);

CREATE TABLE Course (
    Id UUID NOT NULL,
    DisciplineId UUID NOT NULL,
    TeacherId VARCHAR(450) NOT NULL,
    TeacherName VARCHAR(200) NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description TEXT NOT NULL,
    Price DECIMAL(18,2),
    IsFree BOOLEAN NOT NULL,
    IsPublished BOOLEAN NOT NULL DEFAULT FALSE,
    IsArchived BOOLEAN NOT NULL DEFAULT FALSE,
    OrderType VARCHAR(20) NOT NULL,
    HasGrading BOOLEAN NOT NULL DEFAULT TRUE,
    Level VARCHAR(20) NOT NULL,
    ImageUrl VARCHAR(500),
    Tags VARCHAR(500),
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT PK_Course PRIMARY KEY (Id),
    CONSTRAINT FK_Course_Discipline FOREIGN KEY (DisciplineId) REFERENCES Discipline(Id),
    CONSTRAINT FK_Course_Teacher FOREIGN KEY (TeacherId) REFERENCES ApplicationUser(Id)
);

CREATE TABLE CourseEnrollment (
    Id UUID NOT NULL,
    CourseId UUID NOT NULL,
    StudentId VARCHAR(450) NOT NULL,
    EnrolledAt TIMESTAMP NOT NULL,
    Status VARCHAR(20) NOT NULL,
    CONSTRAINT PK_CourseEnrollment PRIMARY KEY (Id),
    CONSTRAINT UQ_CourseEnrollment UNIQUE (CourseId, StudentId),
    CONSTRAINT FK_Enrollment_Course FOREIGN KEY (CourseId) REFERENCES Course(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Enrollment_Student FOREIGN KEY (StudentId) REFERENCES ApplicationUser(Id)
);

CREATE TABLE CourseModule (
    Id UUID NOT NULL,
    CourseId UUID NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    OrderIndex INTEGER NOT NULL,
    IsPublished BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT PK_CourseModule PRIMARY KEY (Id),
    CONSTRAINT FK_Module_Course FOREIGN KEY (CourseId) REFERENCES Course(Id) ON DELETE CASCADE
);

CREATE TABLE Lesson (
    Id UUID NOT NULL,
    ModuleId UUID NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    OrderIndex INTEGER NOT NULL,
    IsPublished BOOLEAN NOT NULL DEFAULT FALSE,
    Duration INTEGER,
    CONSTRAINT PK_Lesson PRIMARY KEY (Id),
    CONSTRAINT FK_Lesson_Module FOREIGN KEY (ModuleId) REFERENCES CourseModule(Id) ON DELETE CASCADE
);

CREATE TABLE LessonBlock (
    Id UUID NOT NULL,
    LessonId UUID NOT NULL,
    OrderIndex INTEGER NOT NULL,
    Type VARCHAR(20) NOT NULL,
    TextContent TEXT,
    VideoUrl VARCHAR(500),
    TestId UUID,
    AssignmentId UUID,
    CONSTRAINT PK_LessonBlock PRIMARY KEY (Id),
    CONSTRAINT FK_Block_Lesson FOREIGN KEY (LessonId) REFERENCES Lesson(Id) ON DELETE CASCADE
);

-- ======================== SCHEMA: content ========================

CREATE TABLE Attachment (
    Id UUID NOT NULL,
    FileName VARCHAR(500) NOT NULL,
    StoragePath VARCHAR(1000) NOT NULL,
    FileUrl VARCHAR(1000) NOT NULL,
    ContentType VARCHAR(100) NOT NULL,
    FileSize BIGINT NOT NULL,
    EntityType VARCHAR(50) NOT NULL,
    EntityId UUID NOT NULL,
    UploadedById VARCHAR(450) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    CONSTRAINT PK_Attachment PRIMARY KEY (Id),
    CONSTRAINT FK_Attachment_Uploader FOREIGN KEY (UploadedById) REFERENCES ApplicationUser(Id)
);

-- ======================== SCHEMA: tests ========================

CREATE TABLE Test (
    Id UUID NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    CreatedById VARCHAR(450) NOT NULL,
    TimeLimitMinutes INTEGER,
    MaxAttempts INTEGER,
    Deadline TIMESTAMP,
    ShuffleQuestions BOOLEAN NOT NULL DEFAULT FALSE,
    ShuffleAnswers BOOLEAN NOT NULL DEFAULT FALSE,
    ShowCorrectAnswers BOOLEAN NOT NULL DEFAULT TRUE,
    MaxScore INTEGER NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT PK_Test PRIMARY KEY (Id),
    CONSTRAINT FK_Test_Creator FOREIGN KEY (CreatedById) REFERENCES ApplicationUser(Id)
);

CREATE TABLE Question (
    Id UUID NOT NULL,
    TestId UUID NOT NULL,
    Type VARCHAR(30) NOT NULL,
    Text TEXT NOT NULL,
    Points INTEGER NOT NULL,
    OrderIndex INTEGER NOT NULL,
    CONSTRAINT PK_Question PRIMARY KEY (Id),
    CONSTRAINT FK_Question_Test FOREIGN KEY (TestId) REFERENCES Test(Id) ON DELETE CASCADE
);

CREATE TABLE AnswerOption (
    Id UUID NOT NULL,
    QuestionId UUID NOT NULL,
    Text VARCHAR(1000) NOT NULL,
    IsCorrect BOOLEAN NOT NULL,
    OrderIndex INTEGER NOT NULL,
    MatchingPairValue VARCHAR(500),
    CONSTRAINT PK_AnswerOption PRIMARY KEY (Id),
    CONSTRAINT FK_Option_Question FOREIGN KEY (QuestionId) REFERENCES Question(Id) ON DELETE CASCADE
);

CREATE TABLE TestAttempt (
    Id UUID NOT NULL,
    TestId UUID NOT NULL,
    StudentId VARCHAR(450) NOT NULL,
    AttemptNumber INTEGER NOT NULL,
    StartedAt TIMESTAMP NOT NULL,
    CompletedAt TIMESTAMP,
    Score INTEGER,
    Status VARCHAR(20) NOT NULL,
    CONSTRAINT PK_TestAttempt PRIMARY KEY (Id),
    CONSTRAINT FK_Attempt_Test FOREIGN KEY (TestId) REFERENCES Test(Id),
    CONSTRAINT FK_Attempt_Student FOREIGN KEY (StudentId) REFERENCES ApplicationUser(Id)
);

CREATE TABLE TestResponse (
    Id UUID NOT NULL,
    AttemptId UUID NOT NULL,
    QuestionId UUID NOT NULL,
    SelectedOptionIds TEXT,
    TextAnswer TEXT,
    IsCorrect BOOLEAN,
    Points INTEGER,
    TeacherComment TEXT,
    CONSTRAINT PK_TestResponse PRIMARY KEY (Id),
    CONSTRAINT UQ_Response UNIQUE (AttemptId, QuestionId),
    CONSTRAINT FK_Response_Attempt FOREIGN KEY (AttemptId) REFERENCES TestAttempt(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Response_Question FOREIGN KEY (QuestionId) REFERENCES Question(Id)
);

-- ======================== SCHEMA: assignments ========================

CREATE TABLE Assignment (
    Id UUID NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description TEXT NOT NULL,
    Criteria TEXT,
    Deadline TIMESTAMP,
    MaxAttempts INTEGER,
    MaxScore INTEGER NOT NULL,
    CreatedById VARCHAR(450) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT PK_Assignment PRIMARY KEY (Id),
    CONSTRAINT FK_Assignment_Creator FOREIGN KEY (CreatedById) REFERENCES ApplicationUser(Id)
);

CREATE TABLE AssignmentSubmission (
    Id UUID NOT NULL,
    AssignmentId UUID NOT NULL,
    StudentId VARCHAR(450) NOT NULL,
    AttemptNumber INTEGER NOT NULL,
    Content TEXT,
    SubmittedAt TIMESTAMP NOT NULL,
    Status VARCHAR(30) NOT NULL,
    Score INTEGER,
    TeacherComment TEXT,
    GradedAt TIMESTAMP,
    GradedById VARCHAR(450),
    CONSTRAINT PK_Submission PRIMARY KEY (Id),
    CONSTRAINT UQ_Submission UNIQUE (AssignmentId, StudentId, AttemptNumber),
    CONSTRAINT FK_Submission_Assignment FOREIGN KEY (AssignmentId) REFERENCES Assignment(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Submission_Student FOREIGN KEY (StudentId) REFERENCES ApplicationUser(Id)
);

-- ======================== SCHEMA: grading ========================

CREATE TABLE Grade (
    Id UUID NOT NULL,
    StudentId VARCHAR(450) NOT NULL,
    CourseId UUID NOT NULL,
    SourceType VARCHAR(20) NOT NULL,
    TestAttemptId UUID,
    AssignmentSubmissionId UUID,
    Title VARCHAR(200) NOT NULL,
    Score DECIMAL(18,2) NOT NULL,
    MaxScore DECIMAL(18,2) NOT NULL,
    Comment TEXT,
    GradedAt TIMESTAMP NOT NULL,
    GradedById VARCHAR(450),
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT PK_Grade PRIMARY KEY (Id),
    CONSTRAINT FK_Grade_Student FOREIGN KEY (StudentId) REFERENCES ApplicationUser(Id),
    CONSTRAINT FK_Grade_Course FOREIGN KEY (CourseId) REFERENCES Course(Id)
);

-- ======================== SCHEMA: progress ========================

CREATE TABLE LessonProgress (
    Id UUID NOT NULL,
    LessonId UUID NOT NULL,
    StudentId VARCHAR(450) NOT NULL,
    IsCompleted BOOLEAN NOT NULL DEFAULT FALSE,
    CompletedAt TIMESTAMP,
    CONSTRAINT PK_LessonProgress PRIMARY KEY (Id),
    CONSTRAINT UQ_Progress UNIQUE (LessonId, StudentId),
    CONSTRAINT FK_Progress_Lesson FOREIGN KEY (LessonId) REFERENCES Lesson(Id),
    CONSTRAINT FK_Progress_Student FOREIGN KEY (StudentId) REFERENCES ApplicationUser(Id)
);

-- ======================== SCHEMA: notifications ========================

CREATE TABLE Notification (
    Id UUID NOT NULL,
    UserId VARCHAR(450) NOT NULL,
    Type VARCHAR(20) NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Message TEXT NOT NULL,
    IsRead BOOLEAN NOT NULL DEFAULT FALSE,
    LinkUrl VARCHAR(500),
    CreatedAt TIMESTAMP NOT NULL,
    CONSTRAINT PK_Notification PRIMARY KEY (Id),
    CONSTRAINT FK_Notification_User FOREIGN KEY (UserId) REFERENCES ApplicationUser(Id)
);

-- ======================== SCHEMA: calendar ========================

CREATE TABLE CalendarEvent (
    Id UUID NOT NULL,
    UserId VARCHAR(450),
    CourseId UUID,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    EventDate TIMESTAMP NOT NULL,
    EventTime VARCHAR(10),
    Type VARCHAR(20) NOT NULL,
    SourceType VARCHAR(50),
    SourceId UUID,
    CreatedAt TIMESTAMP NOT NULL,
    CONSTRAINT PK_CalendarEvent PRIMARY KEY (Id)
);

-- ======================== SCHEMA: scheduling ========================

CREATE TABLE ScheduleSlot (
    Id UUID NOT NULL,
    TeacherId VARCHAR(450) NOT NULL,
    TeacherName VARCHAR(200) NOT NULL,
    CourseId UUID,
    CourseName VARCHAR(200),
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    StartTime TIMESTAMP NOT NULL,
    EndTime TIMESTAMP NOT NULL,
    IsGroupSession BOOLEAN NOT NULL,
    MaxStudents INTEGER NOT NULL DEFAULT 1,
    Status VARCHAR(20) NOT NULL,
    MeetingLink VARCHAR(500),
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT PK_ScheduleSlot PRIMARY KEY (Id),
    CONSTRAINT FK_Slot_Teacher FOREIGN KEY (TeacherId) REFERENCES ApplicationUser(Id)
);

CREATE TABLE SessionBooking (
    Id UUID NOT NULL,
    SlotId UUID NOT NULL,
    StudentId VARCHAR(450) NOT NULL,
    StudentName VARCHAR(200) NOT NULL,
    BookedAt TIMESTAMP NOT NULL,
    Status VARCHAR(20) NOT NULL,
    CONSTRAINT PK_SessionBooking PRIMARY KEY (Id),
    CONSTRAINT UQ_Booking UNIQUE (SlotId, StudentId),
    CONSTRAINT FK_Booking_Slot FOREIGN KEY (SlotId) REFERENCES ScheduleSlot(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Booking_Student FOREIGN KEY (StudentId) REFERENCES ApplicationUser(Id)
);

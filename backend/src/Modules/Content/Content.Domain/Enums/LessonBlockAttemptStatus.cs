// LessonBlockAttemptStatus.cs

namespace Content.Domain.Enums;

// Доменное перечисление enum: фиксирует допустимые состояния и режимы без строковых литералов.
public enum LessonBlockAttemptStatus
{
    Draft = 0,
    Submitted = 1,
    Graded = 2
}

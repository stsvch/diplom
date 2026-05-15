// EnrollmentStatus.cs

namespace Courses.Domain.Enums;

// Доменное перечисление enum: фиксирует допустимые состояния и режимы без строковых литералов.
public enum EnrollmentStatus
{
    Active,
    Completed,
    Dropped
}

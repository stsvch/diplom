// CourseItemType.cs

namespace Courses.Domain.Enums;

// Доменное перечисление enum: фиксирует допустимые состояния и режимы без строковых литералов.
public enum CourseItemType
{
    Lesson,
    Test,
    Assignment,
    Resource,
    ExternalLink
}

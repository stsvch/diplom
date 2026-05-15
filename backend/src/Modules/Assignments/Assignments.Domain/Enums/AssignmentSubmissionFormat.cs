// AssignmentSubmissionFormat.cs

namespace Assignments.Domain.Enums;

/// <summary>
/// Формат сдачи задаёт, что преподаватель разрешил студенту отправлять: текст, файл или оба варианта.
/// </summary>
public enum AssignmentSubmissionFormat
{
    /// Студент сдаёт текстовый ответ.
    Text,

    /// Студент прикладывает файл (документ, архив, презентация).
    File,

    /// Допустим и текстовый ответ, и файл.
    Both
}

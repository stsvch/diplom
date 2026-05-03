namespace Assignments.Domain.Enums;

public enum AssignmentSubmissionFormat
{
    /// <summary>
    /// Студент сдаёт текстовый ответ.
    /// </summary>
    Text,

    /// <summary>
    /// Студент прикладывает файл (документ, архив, презентация).
    /// </summary>
    File,

    /// <summary>
    /// Допустим и текстовый ответ, и файл.
    /// </summary>
    Both
}

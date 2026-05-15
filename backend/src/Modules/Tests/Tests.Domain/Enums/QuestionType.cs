// QuestionType.cs

namespace Tests.Domain.Enums;

/// <summary>
/// Тип QuestionType относится к основному сценарию модуля и документирует его публичный контракт.
/// </summary>
public enum QuestionType
{
    SingleChoice,
    MultipleChoice,
    TextInput,
    Matching,
    OpenAnswer,
    /// Студент пишет код. Проверяется вручную или по ожидаемому выводу.
    Code
}

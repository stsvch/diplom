namespace Tests.Domain.Enums;

public enum QuestionType
{
    SingleChoice,
    MultipleChoice,
    TextInput,
    Matching,
    OpenAnswer,
    /// <summary>
    /// Студент пишет код. Проверяется вручную или по ожидаемому выводу.
    /// </summary>
    Code
}

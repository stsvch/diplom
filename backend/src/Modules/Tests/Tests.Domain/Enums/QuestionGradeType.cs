namespace Tests.Domain.Enums;

public enum QuestionGradeType
{
    /// <summary>
    /// Автопроверка системой (например, выбор правильного варианта).
    /// </summary>
    Auto,

    /// <summary>
    /// Ручная проверка преподавателем (для открытых ответов и кода).
    /// </summary>
    Manual
}

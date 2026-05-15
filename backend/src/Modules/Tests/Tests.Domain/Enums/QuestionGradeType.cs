// QuestionGradeType.cs

namespace Tests.Domain.Enums;

/// <summary>
/// Тип QuestionGradeType относится к основному сценарию модуля и документирует его публичный контракт.
/// </summary>
public enum QuestionGradeType
{
    /// Автопроверка системой (например, выбор правильного варианта).
    Auto,

    /// Ручная проверка преподавателем (для открытых ответов и кода).
    Manual
}

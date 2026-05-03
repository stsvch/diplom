using EduPlatform.Shared.Domain;
using Tests.Domain.Enums;

namespace Tests.Domain.Entities;

public class Question : BaseEntity
{
    public Guid TestId { get; set; }
    public QuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int OrderIndex { get; set; }

    /// <summary>
    /// Авто (выбор варианта) или ручная (открытый ответ, код).
    /// </summary>
    public QuestionGradeType GradeType { get; set; } = QuestionGradeType.Auto;

    /// <summary>
    /// Пояснение к правильному ответу — показывается студенту после прохождения.
    /// </summary>
    public string? Explanation { get; set; }

    /// <summary>
    /// Эталонный ответ для подсказки преподавателю при ручной проверке (для текста и кода).
    /// </summary>
    public string? ExpectedAnswer { get; set; }

    public Test Test { get; set; } = null!;
    public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
}

// PublishValidationResult.cs

namespace Courses.Application.Courses.Commands.PublishCourse;

// Модель проблемы публикации курса: хранит тип, путь, код и понятное сообщение для преподавателя.
public record PublishIssue(string Type, string Path, string Code, string Message);

// Результат проверки публикации: показывает, можно ли публиковать курс и какие блокирующие проблемы найдены.
public class PublishValidationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<PublishIssue> Issues { get; set; } = new();

    public bool HasErrors => Issues.Any(i => i.Type == "error");
    public bool HasWarnings => Issues.Any(i => i.Type == "warning");
}

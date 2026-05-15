// LessonBlockSettings.cs

namespace Content.Domain.ValueObjects.Blocks;

// Value object class: описывает структуру JSON-данных блока урока.
public class LessonBlockSettings
{
    public decimal Points { get; set; } = 1.0m;
    public bool RequiredForCompletion { get; set; } = true;
    public string? Hint { get; set; }
    public bool ShuffleOptions { get; set; }
    public bool ShowFeedback { get; set; } = true;
    public int? MaxAttempts { get; set; }
}

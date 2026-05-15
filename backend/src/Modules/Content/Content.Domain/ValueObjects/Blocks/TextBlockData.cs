// TextBlockData.cs

using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

// Value object class: описывает структуру JSON-данных блока урока.
public class TextBlockData : LessonBlockData
{
    public override LessonBlockType Type => LessonBlockType.Text;
    public string Html { get; set; } = string.Empty;
}

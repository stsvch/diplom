using System.Text.Json.Serialization;
using Content.Domain.Enums;

namespace Content.Domain.ValueObjects.Blocks;

// Полиморфизм обрабатывается явным конвертером LessonBlockDataLenientConverter
// (зарегистрирован в Content.Infrastructure и в API JsonOptions). Конвертер сам:
//   - пишет $type первым свойством;
//   - при чтении принимает $type или legacy type.
// Атрибуты [JsonPolymorphic]/[JsonDerivedType] здесь не используются, чтобы не конфликтовать
// с кастомным конвертером (System.Text.Json не позволяет совмещать атрибутный полиморфизм
// и пользовательский JsonConverter на одном базовом типе).
public abstract class LessonBlockData
{
    [JsonIgnore]
    public abstract LessonBlockType Type { get; }
}

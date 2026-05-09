using System.Text.Json;
using System.Text.Json.Serialization;
using Content.Domain.Enums;
using Content.Domain.ValueObjects.Blocks;

namespace Content.Infrastructure.Persistence.JsonConverters;

/// <summary>
/// Терпимый конвертер LessonBlockData для хранения в jsonb.
/// При чтении принимает оба варианта дискриминатора:
///   - $type (текущий формат, как в API)
///   - type  (исторический формат старых строк в БД)
/// При записи всегда выводит $type первым свойством — это требование System.Text.Json
/// для полиморфной десериализации.
/// </summary>
public class LessonBlockDataLenientConverter : JsonConverter<LessonBlockData>
{
    private static readonly Dictionary<string, Type> DiscriminatorToType = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(LessonBlockType.Text)] = typeof(TextBlockData),
        [nameof(LessonBlockType.Video)] = typeof(VideoBlockData),
        [nameof(LessonBlockType.Audio)] = typeof(AudioBlockData),
        [nameof(LessonBlockType.Image)] = typeof(ImageBlockData),
        [nameof(LessonBlockType.Banner)] = typeof(BannerBlockData),
        [nameof(LessonBlockType.File)] = typeof(FileBlockData),
        [nameof(LessonBlockType.SingleChoice)] = typeof(SingleChoiceBlockData),
        [nameof(LessonBlockType.MultipleChoice)] = typeof(MultipleChoiceBlockData),
        [nameof(LessonBlockType.TrueFalse)] = typeof(TrueFalseBlockData),
        [nameof(LessonBlockType.FillGap)] = typeof(FillGapBlockData),
        [nameof(LessonBlockType.Dropdown)] = typeof(DropdownBlockData),
        [nameof(LessonBlockType.WordBank)] = typeof(WordBankBlockData),
        [nameof(LessonBlockType.Reorder)] = typeof(ReorderBlockData),
        [nameof(LessonBlockType.Matching)] = typeof(MatchingBlockData),
        [nameof(LessonBlockType.OpenText)] = typeof(OpenTextBlockData),
        [nameof(LessonBlockType.CodeExercise)] = typeof(CodeExerciseBlockData),
        [nameof(LessonBlockType.Quiz)] = typeof(QuizBlockData),
        [nameof(LessonBlockType.Assignment)] = typeof(AssignmentBlockData),
    };

    public override LessonBlockData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("LessonBlockData expects an object");

        string? discriminator = null;
        if (root.TryGetProperty("$type", out var dt) && dt.ValueKind == JsonValueKind.String)
        {
            discriminator = dt.GetString();
        }
        else if (root.TryGetProperty("type", out var t) && t.ValueKind == JsonValueKind.String)
        {
            discriminator = t.GetString();
        }

        if (string.IsNullOrWhiteSpace(discriminator))
            throw new JsonException("LessonBlockData missing type discriminator ($type or type)");

        if (!DiscriminatorToType.TryGetValue(discriminator!, out var concreteType))
            throw new JsonException($"Unknown LessonBlockData discriminator: {discriminator}");

        // Десериализуем в конкретный тип напрямую — полиморфизм через атрибуты тут не сработает (тип уже известен).
        var raw = root.GetRawText();
        return (LessonBlockData?)JsonSerializer.Deserialize(raw, concreteType, options);
    }

    public override void Write(Utf8JsonWriter writer, LessonBlockData value, JsonSerializerOptions options)
    {
        var concreteType = value.GetType();
        var element = JsonSerializer.SerializeToElement(value, concreteType, options);

        writer.WriteStartObject();
        writer.WriteString("$type", value.Type.ToString());
        foreach (var prop in element.EnumerateObject())
        {
            // $type и type не дублируем — type из атрибута [JsonIgnore], но на всякий случай.
            if (prop.NameEquals("$type") || prop.NameEquals("type")) continue;
            prop.WriteTo(writer);
        }
        writer.WriteEndObject();
    }
}

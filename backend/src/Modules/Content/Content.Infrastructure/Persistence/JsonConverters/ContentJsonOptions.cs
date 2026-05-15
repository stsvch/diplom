// ContentJsonOptions.cs

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Content.Infrastructure.Persistence.JsonConverters;

// Инфраструктурный JSON-компонент class: настраивает сериализацию value objects для jsonb и API.
public static class ContentJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(),
            // Терпимый конвертер: при чтении принимает $type или старый type, при записи выводит $type первым.
            new LessonBlockDataLenientConverter(),
        }
    };
}

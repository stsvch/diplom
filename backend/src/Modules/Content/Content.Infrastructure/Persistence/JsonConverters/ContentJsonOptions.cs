using System.Text.Json;
using System.Text.Json.Serialization;

namespace Content.Infrastructure.Persistence.JsonConverters;

public static class ContentJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };
}

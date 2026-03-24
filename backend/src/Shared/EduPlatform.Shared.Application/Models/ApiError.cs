using System.Text.Json.Serialization;

namespace EduPlatform.Shared.Application.Models;

public class ApiError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string[]>? Errors { get; set; }

    public static ApiError FromMessage(string message, string? code = null)
        => new() { Message = message, Code = code };

    public static ApiError FromValidation(Dictionary<string, string[]> errors)
        => new() { Message = "Ошибка валидации.", Code = "VALIDATION_ERROR", Errors = errors };
}

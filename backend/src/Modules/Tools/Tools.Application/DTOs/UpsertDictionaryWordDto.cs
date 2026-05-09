namespace Tools.Application.DTOs;

public record UpsertDictionaryWordDto(
    Guid CourseId,
    string Term,
    string? Translation,
    string? Definition,
    string? Example,
    string? Note,
    IReadOnlyCollection<string>? Tags);

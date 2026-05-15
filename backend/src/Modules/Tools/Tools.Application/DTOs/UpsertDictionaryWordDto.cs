// Файл: UpsertDictionaryWordDto.cs
namespace Tools.Application.DTOs;

// DTO UpsertDictionaryWordDto переносит данные наружу без раскрытия доменной сущности.
public record UpsertDictionaryWordDto(
    Guid CourseId,
    string Term,
    string? Translation,
    string? Definition,
    string? Example,
    string? Note,
    IReadOnlyCollection<string>? Tags);

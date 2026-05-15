// SearchTagsQuery.cs

using MediatR;

namespace Courses.Application.Tags.Queries.SearchTags;

// Компонент тегов record: обслуживает поиск, DTO или синхронизацию тегов курса.
public record SearchTagsQuery(string? Query, int Limit) : IRequest<List<TagDto>>;

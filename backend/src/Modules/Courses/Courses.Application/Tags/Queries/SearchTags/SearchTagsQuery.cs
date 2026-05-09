using MediatR;

namespace Courses.Application.Tags.Queries.SearchTags;

public record SearchTagsQuery(string? Query, int Limit) : IRequest<List<TagDto>>;

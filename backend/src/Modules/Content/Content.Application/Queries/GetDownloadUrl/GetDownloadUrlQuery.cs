// GetDownloadUrlQuery.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Queries.GetDownloadUrl;

// Тип record: ключевой элемент файла GetDownloadUrlQuery.cs.
public record GetDownloadUrlQuery(Guid Id) : IRequest<Result<string>>;

using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Queries.GetDownloadUrl;

public record GetDownloadUrlQuery(Guid Id) : IRequest<Result<string>>;

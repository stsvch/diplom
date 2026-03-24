using Content.Application.DTOs;
using MediatR;

namespace Content.Application.Queries.GetEntityFiles;

public record GetEntityFilesQuery(string EntityType, Guid EntityId) : IRequest<List<AttachmentDto>>;

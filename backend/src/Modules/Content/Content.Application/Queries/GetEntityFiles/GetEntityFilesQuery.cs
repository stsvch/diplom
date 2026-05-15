// GetEntityFilesQuery.cs

using Content.Application.DTOs;
using MediatR;

namespace Content.Application.Queries.GetEntityFiles;

// Тип record: ключевой элемент файла GetEntityFilesQuery.cs.
public record GetEntityFilesQuery(string EntityType, Guid EntityId) : IRequest<List<AttachmentDto>>;

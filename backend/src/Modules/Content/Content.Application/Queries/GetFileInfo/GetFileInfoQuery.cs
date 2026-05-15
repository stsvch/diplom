// GetFileInfoQuery.cs

using Content.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Queries.GetFileInfo;

// Тип record: ключевой элемент файла GetFileInfoQuery.cs.
public record GetFileInfoQuery(Guid Id) : IRequest<Result<AttachmentDto>>;

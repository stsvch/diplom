using Content.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Queries.GetFileInfo;

public record GetFileInfoQuery(Guid Id) : IRequest<Result<AttachmentDto>>;

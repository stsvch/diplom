using AutoMapper;
using Content.Application.DTOs;
using Content.Application.Interfaces;
using Content.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.Queries.GetEntityFiles;

public class GetEntityFilesQueryHandler : IRequestHandler<GetEntityFilesQuery, List<AttachmentDto>>
{
    private readonly IContentDbContext _context;
    private readonly IMapper _mapper;

    public GetEntityFilesQueryHandler(IContentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AttachmentDto>> Handle(GetEntityFilesQuery request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AttachmentEntityType>(request.EntityType, ignoreCase: true, out var entityType))
            return new List<AttachmentDto>();

        var attachments = await _context.Attachments
            .Where(a => a.EntityType == entityType && a.EntityId == request.EntityId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<AttachmentDto>>(attachments);
    }
}

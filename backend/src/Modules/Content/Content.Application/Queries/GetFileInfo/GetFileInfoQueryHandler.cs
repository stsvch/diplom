using AutoMapper;
using Content.Application.DTOs;
using Content.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.Queries.GetFileInfo;

public class GetFileInfoQueryHandler : IRequestHandler<GetFileInfoQuery, Result<AttachmentDto>>
{
    private readonly IContentDbContext _context;
    private readonly IMapper _mapper;

    public GetFileInfoQueryHandler(IContentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<AttachmentDto>> Handle(GetFileInfoQuery request, CancellationToken cancellationToken)
    {
        var attachment = await _context.Attachments
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (attachment == null)
            return Result.Failure<AttachmentDto>("File not found.");

        return Result.Success(_mapper.Map<AttachmentDto>(attachment));
    }
}

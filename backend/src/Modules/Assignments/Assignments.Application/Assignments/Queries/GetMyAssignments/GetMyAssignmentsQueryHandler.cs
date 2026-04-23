using Assignments.Application.DTOs;
using Assignments.Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Assignments.Queries.GetMyAssignments;

public class GetMyAssignmentsQueryHandler : IRequestHandler<GetMyAssignmentsQuery, List<AssignmentDto>>
{
    private readonly IAssignmentsDbContext _db;
    private readonly IMapper _mapper;

    public GetMyAssignmentsQueryHandler(IAssignmentsDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<AssignmentDto>> Handle(GetMyAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var list = await _db.Assignments
            .AsNoTracking()
            .Where(a => a.CreatedById == request.TeacherId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<AssignmentDto>>(list);
    }
}

using Assignments.Application.DTOs;
using Assignments.Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Assignments.Queries.GetSubmissions;

public class GetSubmissionsQueryHandler : IRequestHandler<GetSubmissionsQuery, List<SubmissionDto>>
{
    private readonly IAssignmentsDbContext _db;
    private readonly IMapper _mapper;

    public GetSubmissionsQueryHandler(IAssignmentsDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<List<SubmissionDto>> Handle(GetSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken);
        if (assignment is null || assignment.CreatedById != request.TeacherId)
            return [];

        var submissions = await _db.AssignmentSubmissions
            .Where(s => s.AssignmentId == request.AssignmentId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<SubmissionDto>>(submissions);
        foreach (var dto in dtos) dto.MaxScore = assignment.MaxScore;
        return dtos;
    }
}

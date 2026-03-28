using Assignments.Application.DTOs;
using Assignments.Application.Interfaces;
using Assignments.Domain.Enums;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Assignments.Queries.GetPendingSubmissions;

public class GetPendingSubmissionsQueryHandler : IRequestHandler<GetPendingSubmissionsQuery, List<SubmissionDto>>
{
    private readonly IAssignmentsDbContext _db;
    private readonly IMapper _mapper;

    public GetPendingSubmissionsQueryHandler(IAssignmentsDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<List<SubmissionDto>> Handle(GetPendingSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var submissions = await _db.AssignmentSubmissions
            .Include(s => s.Assignment)
            .Where(s => s.Assignment.CreatedById == request.TeacherId &&
                        (s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview))
            .OrderBy(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<SubmissionDto>>(submissions);
        for (int i = 0; i < dtos.Count; i++)
            dtos[i].MaxScore = submissions[i].Assignment.MaxScore;
        return dtos;
    }
}

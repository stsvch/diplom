using Assignments.Application.DTOs;
using Assignments.Application.Interfaces;
using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Assignments.Queries.GetAssignmentById;

public class GetAssignmentByIdQueryHandler : IRequestHandler<GetAssignmentByIdQuery, Result<AssignmentDetailDto>>
{
    private readonly IAssignmentsDbContext _db;
    private readonly IMapper _mapper;

    public GetAssignmentByIdQueryHandler(IAssignmentsDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<Result<AssignmentDetailDto>> Handle(GetAssignmentByIdQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Submissions)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (assignment is null) return Result.Failure<AssignmentDetailDto>("Задание не найдено.");

        return Result.Success(_mapper.Map<AssignmentDetailDto>(assignment));
    }
}

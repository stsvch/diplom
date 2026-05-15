// GetMySubmissionsQueryHandler.cs

using Assignments.Application.DTOs;
using Assignments.Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Assignments.Queries.GetMySubmissions;

/// <summary>
/// Обработчик сценария: возвращает историю сдач студента по конкретному заданию с максимальным баллом задания.
/// </summary>
public class GetMySubmissionsQueryHandler : IRequestHandler<GetMySubmissionsQuery, List<SubmissionDto>>
{
    private readonly IAssignmentsDbContext _db;
    private readonly IMapper _mapper;

    public GetMySubmissionsQueryHandler(IAssignmentsDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    // Последовательно выполняет сценарий: возвращает историю сдач студента по конкретному заданию с максимальным баллом задания.
    public async Task<List<SubmissionDto>> Handle(GetMySubmissionsQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken);
        if (assignment is null) return [];

        var submissions = await _db.AssignmentSubmissions
            .Where(s => s.AssignmentId == request.AssignmentId && s.StudentId == request.StudentId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<SubmissionDto>>(submissions);
        foreach (var dto in dtos) dto.MaxScore = assignment.MaxScore;
        return dtos;
    }
}

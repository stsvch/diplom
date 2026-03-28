using Assignments.Application.DTOs;
using Assignments.Application.Interfaces;
using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Assignments.Commands.UpdateAssignment;

public class UpdateAssignmentCommandHandler : IRequestHandler<UpdateAssignmentCommand, Result<AssignmentDto>>
{
    private readonly IAssignmentsDbContext _context;
    private readonly IMapper _mapper;

    public UpdateAssignmentCommandHandler(IAssignmentsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<AssignmentDto>> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.Assignments
            .Include(a => a.Submissions)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (assignment is null)
            return Result.Failure<AssignmentDto>("Задание не найдено.");

        if (assignment.CreatedById != request.CreatedById)
            return Result.Failure<AssignmentDto>("Вы не являетесь автором этого задания.");

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.Criteria = request.Criteria;
        assignment.Deadline = request.Deadline;
        assignment.MaxAttempts = request.MaxAttempts;
        assignment.MaxScore = request.MaxScore;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<AssignmentDto>(assignment);
        return Result.Success(dto);
    }
}

using Assignments.Application.DTOs;
using Assignments.Application.Interfaces;
using Assignments.Domain.Entities;
using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Assignments.Application.Assignments.Commands.CreateAssignment;

public class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, Result<AssignmentDto>>
{
    private readonly IAssignmentsDbContext _context;
    private readonly IMapper _mapper;

    public CreateAssignmentCommandHandler(IAssignmentsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<AssignmentDto>> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = new Assignment
        {
            Title = request.Title,
            Description = request.Description,
            Criteria = request.Criteria,
            Deadline = request.Deadline,
            MaxAttempts = request.MaxAttempts,
            MaxScore = request.MaxScore,
            CreatedById = request.CreatedById
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<AssignmentDto>(assignment);
        return Result.Success(dto);
    }
}

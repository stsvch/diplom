using Assignments.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Assignments.Commands.DeleteAssignment;

public class DeleteAssignmentCommandHandler : IRequestHandler<DeleteAssignmentCommand, Result<string>>
{
    private readonly IAssignmentsDbContext _db;

    public DeleteAssignmentCommandHandler(IAssignmentsDbContext db) => _db = db;

    public async Task<Result<string>> Handle(DeleteAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (assignment is null) return Result.Failure<string>("Задание не найдено.");
        if (assignment.CreatedById != request.CreatedById) return Result.Failure<string>("Нет прав на удаление.");

        _db.Assignments.Remove(assignment);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success("Задание удалено.");
    }
}

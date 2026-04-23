using Assignments.Application.Interfaces;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Assignments.Commands.DeleteAssignment;

public class DeleteAssignmentCommandHandler : IRequestHandler<DeleteAssignmentCommand, Result<string>>
{
    private readonly IAssignmentsDbContext _db;
    private readonly ICalendarEventPublisher _calendar;
    private readonly IGradeRecordWriter _grades;

    public DeleteAssignmentCommandHandler(
        IAssignmentsDbContext db,
        ICalendarEventPublisher calendar,
        IGradeRecordWriter grades)
    {
        _db = db;
        _calendar = calendar;
        _grades = grades;
    }

    public async Task<Result<string>> Handle(DeleteAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (assignment is null) return Result.Failure<string>("Задание не найдено.");
        if (assignment.CreatedById != request.CreatedById) return Result.Failure<string>("Нет прав на удаление.");

        var submissionIds = await _db.AssignmentSubmissions
            .Where(s => s.AssignmentId == assignment.Id)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        _db.Assignments.Remove(assignment);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var submissionId in submissionIds)
        {
            await _grades.DeleteByAssignmentSubmissionAsync(submissionId, cancellationToken);
        }

        await _calendar.DeleteBySourceAsync("Assignment", assignment.Id, cancellationToken);

        return Result.Success("Задание удалено.");
    }
}

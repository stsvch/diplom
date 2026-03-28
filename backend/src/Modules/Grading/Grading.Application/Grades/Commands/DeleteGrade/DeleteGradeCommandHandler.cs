using EduPlatform.Shared.Domain;
using Grading.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Grading.Application.Grades.Commands.DeleteGrade;

public class DeleteGradeCommandHandler : IRequestHandler<DeleteGradeCommand, Result>
{
    private readonly IGradingDbContext _context;

    public DeleteGradeCommandHandler(IGradingDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteGradeCommand request, CancellationToken cancellationToken)
    {
        var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (grade is null)
            return Result.Failure("Оценка не найдена.");
        if (grade.GradedById != request.RequesterId)
            return Result.Failure("Нет прав на удаление этой оценки.");

        _context.Grades.Remove(grade);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// DeleteGradeCommandHandler.cs

using EduPlatform.Shared.Domain;
using Grading.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Grading.Application.Grades.Commands.DeleteGrade;

/// <summary>
/// Обработчик CQRS-команды DeleteGradeCommand: выполняет сценарий изменения состояния и сохраняет результат.
/// </summary>
public class DeleteGradeCommandHandler : IRequestHandler<DeleteGradeCommand, Result>
{
    private readonly IGradingDbContext _context;

    public DeleteGradeCommandHandler(IGradingDbContext context) => _context = context;

    // Основной сценарий handler-а: проверки, чтение/изменение данных и возврат результата.
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

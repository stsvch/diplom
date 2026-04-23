using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.Interfaces;

namespace Tests.Application.Tests.Commands.DeleteTest;

public class DeleteTestCommandHandler : IRequestHandler<DeleteTestCommand, Result<string>>
{
    private readonly ITestsDbContext _context;
    private readonly ICalendarEventPublisher _calendar;
    private readonly IGradeRecordWriter _grades;

    public DeleteTestCommandHandler(
        ITestsDbContext context,
        ICalendarEventPublisher calendar,
        IGradeRecordWriter grades)
    {
        _context = context;
        _calendar = calendar;
        _grades = grades;
    }

    public async Task<Result<string>> Handle(DeleteTestCommand request, CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (test is null)
            return Result.Failure<string>("Тест не найден.");

        if (test.CreatedById != request.CreatedById)
            return Result.Failure<string>("Вы не являетесь автором этого теста.");

        var attemptIds = await _context.TestAttempts
            .Where(a => a.TestId == test.Id)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        _context.Tests.Remove(test);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var attemptId in attemptIds)
        {
            await _grades.DeleteByTestAttemptAsync(attemptId, cancellationToken);
        }

        await _calendar.DeleteBySourceAsync("Test", test.Id, cancellationToken);

        return Result.Success<string>("Тест успешно удалён.");
    }
}

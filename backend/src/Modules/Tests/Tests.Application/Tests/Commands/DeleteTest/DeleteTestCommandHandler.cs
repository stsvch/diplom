using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.Interfaces;

namespace Tests.Application.Tests.Commands.DeleteTest;

public class DeleteTestCommandHandler : IRequestHandler<DeleteTestCommand, Result<string>>
{
    private readonly ITestsDbContext _context;

    public DeleteTestCommandHandler(ITestsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(DeleteTestCommand request, CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (test is null)
            return Result.Failure<string>("Тест не найден.");

        if (test.CreatedById != request.CreatedById)
            return Result.Failure<string>("Вы не являетесь автором этого теста.");

        _context.Tests.Remove(test);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Тест успешно удалён.");
    }
}

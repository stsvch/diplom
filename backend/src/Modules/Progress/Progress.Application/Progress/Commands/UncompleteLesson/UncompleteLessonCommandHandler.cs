using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Progress.Application.Interfaces;

namespace Progress.Application.Progress.Commands.UncompleteLesson;

public class UncompleteLessonCommandHandler : IRequestHandler<UncompleteLessonCommand, Result>
{
    private readonly IProgressDbContext _context;

    public UncompleteLessonCommandHandler(IProgressDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UncompleteLessonCommand request, CancellationToken cancellationToken)
    {
        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(p => p.LessonId == request.LessonId && p.StudentId == request.StudentId, cancellationToken);

        if (progress is null)
            return Result.Failure("Progress record not found");

        progress.IsCompleted = false;
        progress.CompletedAt = null;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

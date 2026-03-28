using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Progress.Application.DTOs;
using Progress.Application.Interfaces;
using Progress.Domain.Entities;

namespace Progress.Application.Progress.Commands.CompleteLesson;

public class CompleteLessonCommandHandler : IRequestHandler<CompleteLessonCommand, Result<LessonProgressDto>>
{
    private readonly IProgressDbContext _context;

    public CompleteLessonCommandHandler(IProgressDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LessonProgressDto>> Handle(CompleteLessonCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.LessonProgresses
            .FirstOrDefaultAsync(p => p.LessonId == request.LessonId && p.StudentId == request.StudentId, cancellationToken);

        if (existing is not null)
        {
            if (existing.IsCompleted)
                return Result.Success(MapToDto(existing));

            existing.IsCompleted = true;
            existing.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new LessonProgress
            {
                LessonId = request.LessonId,
                StudentId = request.StudentId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            };
            _context.LessonProgresses.Add(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(MapToDto(existing));
    }

    private static LessonProgressDto MapToDto(LessonProgress p) => new()
    {
        Id = p.Id,
        LessonId = p.LessonId,
        StudentId = p.StudentId,
        IsCompleted = p.IsCompleted,
        CompletedAt = p.CompletedAt
    };
}

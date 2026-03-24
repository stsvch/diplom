using Courses.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.LessonBlocks.Commands.ReorderBlocks;

public class ReorderBlocksCommandHandler : IRequestHandler<ReorderBlocksCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;

    public ReorderBlocksCommandHandler(ICoursesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(ReorderBlocksCommand request, CancellationToken cancellationToken)
    {
        var blocks = await _context.LessonBlocks
            .Where(b => b.LessonId == request.LessonId)
            .ToListAsync(cancellationToken);

        for (var i = 0; i < request.OrderedIds.Count; i++)
        {
            var block = blocks.FirstOrDefault(b => b.Id == request.OrderedIds[i]);
            if (block != null)
                block.OrderIndex = i;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Порядок блоков обновлён.");
    }
}

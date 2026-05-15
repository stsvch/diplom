// ReorderBlocksCommandHandler.cs

using Content.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.LessonBlocks.Commands.ReorderBlocks;

// Тип class: ключевой элемент файла ReorderBlocksCommandHandler.cs.
public class ReorderBlocksCommandHandler : IRequestHandler<ReorderBlocksCommand, Result<string>>
{
    private readonly IContentDbContext _context;

    public ReorderBlocksCommandHandler(IContentDbContext context)
    {
        _context = context;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<Result<string>> Handle(ReorderBlocksCommand request, CancellationToken cancellationToken)
    {
        var blocks = await _context.LessonBlocks
            .Where(b => b.LessonId == request.LessonId)
            .ToListAsync(cancellationToken);

        for (var i = 0; i < request.OrderedIds.Count; i++)
        {
            var block = blocks.FirstOrDefault(b => b.Id == request.OrderedIds[i]);
            if (block is not null)
            {
                block.OrderIndex = i;
                block.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Порядок блоков обновлён.");
    }
}

// DeleteLessonBlockCommandHandler.cs

using Content.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.LessonBlocks.Commands.DeleteLessonBlock;

// Тип class: ключевой элемент файла DeleteLessonBlockCommandHandler.cs.
public class DeleteLessonBlockCommandHandler : IRequestHandler<DeleteLessonBlockCommand, Result<string>>
{
    private readonly IContentDbContext _context;

    public DeleteLessonBlockCommandHandler(IContentDbContext context)
    {
        _context = context;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<Result<string>> Handle(DeleteLessonBlockCommand request, CancellationToken cancellationToken)
    {
        var block = await _context.LessonBlocks.FindAsync([request.Id], cancellationToken);
        if (block is null)
            return Result.Failure<string>("Блок не найден.");

        _context.LessonBlocks.Remove(block);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Блок удалён.");
    }
}

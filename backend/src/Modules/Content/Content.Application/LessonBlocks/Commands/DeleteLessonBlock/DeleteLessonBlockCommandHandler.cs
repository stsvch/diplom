using Content.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.LessonBlocks.Commands.DeleteLessonBlock;

public class DeleteLessonBlockCommandHandler : IRequestHandler<DeleteLessonBlockCommand, Result<string>>
{
    private readonly IContentDbContext _context;

    public DeleteLessonBlockCommandHandler(IContentDbContext context)
    {
        _context = context;
    }

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

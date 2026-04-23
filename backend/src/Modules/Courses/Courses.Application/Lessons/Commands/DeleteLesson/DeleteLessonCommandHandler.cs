using Courses.Application.Interfaces;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Lessons.Commands.DeleteLesson;

public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;
    private readonly ILessonContentCleaner _contentCleaner;

    public DeleteLessonCommandHandler(ICoursesDbContext context, ILessonContentCleaner contentCleaner)
    {
        _context = context;
        _contentCleaner = contentCleaner;
    }

    public async Task<Result<string>> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _context.Lessons.FindAsync([request.Id], cancellationToken);
        if (lesson == null)
            return Result.Failure<string>("Урок не найден.");

        await _contentCleaner.DeleteByLessonIdAsync(lesson.Id, cancellationToken);

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Урок удалён.");
    }
}

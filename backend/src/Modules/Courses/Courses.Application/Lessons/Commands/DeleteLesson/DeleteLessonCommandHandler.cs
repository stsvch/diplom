using Courses.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Lessons.Commands.DeleteLesson;

public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;

    public DeleteLessonCommandHandler(ICoursesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _context.Lessons.FindAsync([request.Id], cancellationToken);
        if (lesson == null)
            return Result.Failure<string>("Урок не найден.");

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Урок удалён.");
    }
}

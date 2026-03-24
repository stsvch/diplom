using Courses.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.ArchiveCourse;

public class ArchiveCourseCommandHandler : IRequestHandler<ArchiveCourseCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;

    public ArchiveCourseCommandHandler(ICoursesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(ArchiveCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses.FindAsync([request.Id], cancellationToken);
        if (course == null)
            return Result.Failure<string>("Курс не найден.");

        if (course.TeacherId != request.TeacherId)
            return Result.Failure<string>("Вы не можете архивировать чужой курс.");

        course.IsArchived = true;
        course.IsPublished = false;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Курс архивирован.");
    }
}

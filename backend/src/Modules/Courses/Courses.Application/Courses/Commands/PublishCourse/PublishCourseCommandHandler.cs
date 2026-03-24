using Courses.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.PublishCourse;

public class PublishCourseCommandHandler : IRequestHandler<PublishCourseCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;

    public PublishCourseCommandHandler(ICoursesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(PublishCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses.FindAsync([request.Id], cancellationToken);
        if (course == null)
            return Result.Failure<string>("Курс не найден.");

        if (course.TeacherId != request.TeacherId)
            return Result.Failure<string>("Вы не можете публиковать чужой курс.");

        course.IsPublished = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Курс опубликован.");
    }
}

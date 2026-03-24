using Courses.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.DeleteCourse;

public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;

    public DeleteCourseCommandHandler(ICoursesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses.FindAsync([request.Id], cancellationToken);
        if (course == null)
            return Result.Failure<string>("Курс не найден.");

        if (course.TeacherId != request.TeacherId)
            return Result.Failure<string>("Вы не можете удалить чужой курс.");

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Курс удалён.");
    }
}

using Courses.Application.Interfaces;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.ArchiveCourse;

public class ArchiveCourseCommandHandler : IRequestHandler<ArchiveCourseCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;
    private readonly ICalendarEventPublisher _calendar;
    private readonly IChatAdmin _chatAdmin;

    public ArchiveCourseCommandHandler(
        ICoursesDbContext context,
        ICalendarEventPublisher calendar,
        IChatAdmin chatAdmin)
    {
        _context = context;
        _calendar = calendar;
        _chatAdmin = chatAdmin;
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

        await _calendar.DeleteByCourseAsync(request.Id, cancellationToken);

        await _chatAdmin.ArchiveCourseChatAsync(request.Id.ToString(), cancellationToken);

        return Result.Success<string>("Курс архивирован.");
    }
}

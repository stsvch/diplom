// RestoreCourseCommandHandler.cs

using Courses.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.RestoreCourse;

// Тип class: ключевой элемент файла RestoreCourseCommandHandler.cs.
public class RestoreCourseCommandHandler : IRequestHandler<RestoreCourseCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;

    public RestoreCourseCommandHandler(ICoursesDbContext context)
    {
        _context = context;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<Result<string>> Handle(RestoreCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses.FindAsync([request.Id], cancellationToken);
        if (course == null)
            return Result.Failure<string>("Курс не найден.");

        if (course.TeacherId != request.TeacherId)
            return Result.Failure<string>("Вы не можете восстановить чужой курс.");

        if (!course.IsArchived)
            return Result.Failure<string>("Курс не находится в архиве.");

        // Возвращаем курс в Draft. Заново публиковать преподаватель будет вручную через
        // обычный flow «Опубликовать», чтобы подтвердить актуальность контента.
        course.IsArchived = false;
        course.IsPublished = false;
        course.ArchiveReason = null;
        course.ArchivedBy = null;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Курс восстановлен из архива.");
    }
}

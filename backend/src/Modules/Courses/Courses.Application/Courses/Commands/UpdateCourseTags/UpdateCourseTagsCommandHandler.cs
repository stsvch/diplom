// UpdateCourseTagsCommandHandler.cs

using Courses.Application.Interfaces;
using Courses.Application.Tags;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Commands.UpdateCourseTags;

// Тип class: ключевой элемент файла UpdateCourseTagsCommandHandler.cs.
public class UpdateCourseTagsCommandHandler : IRequestHandler<UpdateCourseTagsCommand, Result>
{
    private readonly ICoursesDbContext _context;
    private readonly ITagSynchronizer _tagSync;

    public UpdateCourseTagsCommandHandler(ICoursesDbContext context, ITagSynchronizer tagSync)
    {
        _context = context;
        _tagSync = tagSync;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<Result> Handle(UpdateCourseTagsCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .Where(c => c.Id == request.CourseId)
            .Select(c => new { c.Id, c.TeacherId, c.IsArchived })
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            return Result.Failure("Курс не найден.");

        if (course.TeacherId != request.TeacherId)
            return Result.Failure("Вы не можете редактировать чужой курс.");

        if (course.IsArchived)
            return Result.Failure("Архивированный курс нельзя редактировать.");

        await _tagSync.SyncAsync(course.Id, request.Tags, cancellationToken);
        return Result.Success();
    }
}

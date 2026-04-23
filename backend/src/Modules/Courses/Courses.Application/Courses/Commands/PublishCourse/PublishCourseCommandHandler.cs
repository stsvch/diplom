using Courses.Application.Interfaces;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Commands.PublishCourse;

public class PublishCourseCommandHandler : IRequestHandler<PublishCourseCommand, Result<PublishValidationResult>>
{
    private readonly ICoursesDbContext _context;
    private readonly IContentReadService _contentReadService;

    public PublishCourseCommandHandler(ICoursesDbContext context, IContentReadService contentReadService)
    {
        _context = context;
        _contentReadService = contentReadService;
    }

    public async Task<Result<PublishValidationResult>> Handle(PublishCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (course == null)
            return Result.Failure<PublishValidationResult>("Курс не найден.");

        if (course.TeacherId != request.TeacherId)
            return Result.Failure<PublishValidationResult>("Вы не можете публиковать чужой курс.");

        var validation = new PublishValidationResult();

        if (course.Modules.Count == 0)
        {
            validation.Issues.Add(new PublishIssue(
                "error", $"course-{course.Id}", "NO_MODULES",
                "В курсе нет ни одного модуля."));
        }

        foreach (var module in course.Modules)
        {
            if (module.Lessons.Count == 0)
            {
                validation.Issues.Add(new PublishIssue(
                    "error", $"module-{module.Id}", "EMPTY_MODULE",
                    $"В модуле «{module.Title}» нет уроков."));
            }
        }

        var allLessonIds = course.Modules.SelectMany(m => m.Lessons.Select(l => l.Id)).ToList();
        if (allLessonIds.Count > 0)
        {
            var counts = await _contentReadService.GetBlocksCountByLessonIdsAsync(allLessonIds, cancellationToken);
            foreach (var module in course.Modules)
            {
                foreach (var lesson in module.Lessons)
                {
                    counts.TryGetValue(lesson.Id, out var count);
                    if (count == 0)
                    {
                        validation.Issues.Add(new PublishIssue(
                            "warning", $"module-{module.Id}/lesson-{lesson.Id}", "EMPTY_LESSON",
                            $"В уроке «{lesson.Title}» нет блоков."));
                    }
                }
            }
        }

        if (validation.HasErrors && !request.Force)
        {
            validation.Success = false;
            validation.Message = "Курс не может быть опубликован — есть ошибки в структуре.";
            return Result.Success(validation);
        }

        course.IsPublished = true;
        await _context.SaveChangesAsync(cancellationToken);

        validation.Success = true;
        validation.Message = validation.HasWarnings
            ? "Курс опубликован с предупреждениями."
            : "Курс опубликован.";
        return Result.Success(validation);
    }
}

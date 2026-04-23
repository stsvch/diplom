using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Lessons.Commands.UpdateLesson;

public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand, Result<LessonDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public UpdateLessonCommandHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<LessonDto>> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Module)
                .ThenInclude(m => m.Course)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (lesson == null)
            return Result.Failure<LessonDto>("Урок не найден.");

        if (lesson.Module.Course.TeacherId != request.TeacherId)
            return Result.Failure<LessonDto>("Вы не можете редактировать чужой урок.");

        lesson.Title = request.Title;
        lesson.Description = request.Description;
        lesson.Duration = request.Duration;
        if (request.IsPublished.HasValue)
            lesson.IsPublished = request.IsPublished.Value;
        if (request.Layout.HasValue)
            lesson.Layout = request.Layout.Value;

        if (request.ModuleId.HasValue && request.ModuleId.Value != lesson.ModuleId)
        {
            var targetModule = await _context.CourseModules
                .FirstOrDefaultAsync(m => m.Id == request.ModuleId.Value, cancellationToken);
            if (targetModule == null)
                return Result.Failure<LessonDto>("Целевой модуль не найден.");
            if (targetModule.CourseId != lesson.Module.CourseId)
                return Result.Failure<LessonDto>("Нельзя переносить урок в модуль другого курса.");
            lesson.ModuleId = request.ModuleId.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<LessonDto>(lesson));
    }
}

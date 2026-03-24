using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using Courses.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Lessons.Commands.CreateLesson;

public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, Result<LessonDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public CreateLessonCommandHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<LessonDto>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        var moduleExists = await _context.CourseModules.AnyAsync(m => m.Id == request.ModuleId, cancellationToken);
        if (!moduleExists)
            return Result.Failure<LessonDto>("Модуль не найден.");

        var maxOrder = await _context.Lessons
            .Where(l => l.ModuleId == request.ModuleId)
            .MaxAsync(l => (int?)l.OrderIndex, cancellationToken) ?? -1;

        var lesson = new Lesson
        {
            ModuleId = request.ModuleId,
            Title = request.Title,
            Description = request.Description,
            Duration = request.Duration,
            OrderIndex = maxOrder + 1
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<LessonDto>(lesson));
    }
}

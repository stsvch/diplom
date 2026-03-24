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
            .Include(l => l.Blocks)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (lesson == null)
            return Result.Failure<LessonDto>("Урок не найден.");

        lesson.Title = request.Title;
        lesson.Description = request.Description;
        lesson.Duration = request.Duration;
        if (request.IsPublished.HasValue)
            lesson.IsPublished = request.IsPublished.Value;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<LessonDto>(lesson));
    }
}

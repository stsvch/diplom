using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using Courses.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.LessonBlocks.Commands.CreateLessonBlock;

public class CreateLessonBlockCommandHandler : IRequestHandler<CreateLessonBlockCommand, Result<LessonBlockDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public CreateLessonBlockCommandHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<LessonBlockDto>> Handle(CreateLessonBlockCommand request, CancellationToken cancellationToken)
    {
        var lessonExists = await _context.Lessons.AnyAsync(l => l.Id == request.LessonId, cancellationToken);
        if (!lessonExists)
            return Result.Failure<LessonBlockDto>("Урок не найден.");

        var maxOrder = await _context.LessonBlocks
            .Where(b => b.LessonId == request.LessonId)
            .MaxAsync(b => (int?)b.OrderIndex, cancellationToken) ?? -1;

        var block = new LessonBlock
        {
            LessonId = request.LessonId,
            Type = request.Type,
            TextContent = request.TextContent,
            VideoUrl = request.VideoUrl,
            TestId = request.TestId,
            AssignmentId = request.AssignmentId,
            OrderIndex = maxOrder + 1
        };

        _context.LessonBlocks.Add(block);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<LessonBlockDto>(block));
    }
}

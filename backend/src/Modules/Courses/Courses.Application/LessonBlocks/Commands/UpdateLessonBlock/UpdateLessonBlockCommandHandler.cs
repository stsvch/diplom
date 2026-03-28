using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.LessonBlocks.Commands.UpdateLessonBlock;

public class UpdateLessonBlockCommandHandler : IRequestHandler<UpdateLessonBlockCommand, Result<LessonBlockDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public UpdateLessonBlockCommandHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<LessonBlockDto>> Handle(UpdateLessonBlockCommand request, CancellationToken cancellationToken)
    {
        var block = await _context.LessonBlocks.FindAsync([request.Id], cancellationToken);
        if (block == null)
            return Result.Failure<LessonBlockDto>("Блок не найден.");

        block.TextContent = request.TextContent;
        block.VideoUrl = request.VideoUrl;
        block.TestId = request.TestId;
        block.AssignmentId = request.AssignmentId;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<LessonBlockDto>(block));
    }
}

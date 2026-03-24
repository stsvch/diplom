using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Modules.Commands.UpdateModule;

public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand, Result<CourseModuleDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public UpdateModuleCommandHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<CourseModuleDto>> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _context.CourseModules
            .Include(m => m.Lessons)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (module == null)
            return Result.Failure<CourseModuleDto>("Модуль не найден.");

        module.Title = request.Title;
        module.Description = request.Description;
        if (request.IsPublished.HasValue)
            module.IsPublished = request.IsPublished.Value;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<CourseModuleDto>(module));
    }
}

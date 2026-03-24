using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using Courses.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Modules.Commands.CreateModule;

public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, Result<CourseModuleDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public CreateModuleCommandHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<CourseModuleDto>> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == request.CourseId, cancellationToken);
        if (!courseExists)
            return Result.Failure<CourseModuleDto>("Курс не найден.");

        var maxOrder = await _context.CourseModules
            .Where(m => m.CourseId == request.CourseId)
            .MaxAsync(m => (int?)m.OrderIndex, cancellationToken) ?? -1;

        var module = new CourseModule
        {
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            OrderIndex = maxOrder + 1
        };

        _context.CourseModules.Add(module);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<CourseModuleDto>(module));
    }
}

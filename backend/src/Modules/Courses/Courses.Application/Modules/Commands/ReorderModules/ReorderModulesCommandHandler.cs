using Courses.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Modules.Commands.ReorderModules;

public class ReorderModulesCommandHandler : IRequestHandler<ReorderModulesCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;

    public ReorderModulesCommandHandler(ICoursesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(ReorderModulesCommand request, CancellationToken cancellationToken)
    {
        var modules = await _context.CourseModules
            .Where(m => m.CourseId == request.CourseId)
            .ToListAsync(cancellationToken);

        for (var i = 0; i < request.OrderedIds.Count; i++)
        {
            var module = modules.FirstOrDefault(m => m.Id == request.OrderedIds[i]);
            if (module != null)
                module.OrderIndex = i;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Порядок модулей обновлён.");
    }
}

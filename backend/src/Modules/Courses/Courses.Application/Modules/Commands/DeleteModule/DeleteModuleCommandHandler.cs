// DeleteModuleCommandHandler.cs

using Courses.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Modules.Commands.DeleteModule;

// Тип class: ключевой элемент файла DeleteModuleCommandHandler.cs.
public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;

    public DeleteModuleCommandHandler(ICoursesDbContext context)
    {
        _context = context;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<Result<string>> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _context.CourseModules.FindAsync([request.Id], cancellationToken);
        if (module == null)
            return Result.Failure<string>("Модуль не найден.");

        _context.CourseModules.Remove(module);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Модуль удалён.");
    }
}

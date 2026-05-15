// ReorderLessonsCommandHandler.cs

using Courses.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Lessons.Commands.ReorderLessons;

// Тип class: ключевой элемент файла ReorderLessonsCommandHandler.cs.
public class ReorderLessonsCommandHandler : IRequestHandler<ReorderLessonsCommand, Result<string>>
{
    private readonly ICoursesDbContext _context;

    public ReorderLessonsCommandHandler(ICoursesDbContext context)
    {
        _context = context;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<Result<string>> Handle(ReorderLessonsCommand request, CancellationToken cancellationToken)
    {
        var lessons = await _context.Lessons
            .Where(l => l.ModuleId == request.ModuleId)
            .ToListAsync(cancellationToken);

        for (var i = 0; i < request.OrderedIds.Count; i++)
        {
            var lesson = lessons.FirstOrDefault(l => l.Id == request.OrderedIds[i]);
            if (lesson != null)
                lesson.OrderIndex = i;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Порядок уроков обновлён.");
    }
}

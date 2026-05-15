// Файл: GetMyProgressQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Progress.Application.DTOs;
using Progress.Application.Interfaces;

namespace Progress.Application.Progress.Queries.GetMyProgress;

// Обработчик запроса GetMyProgressQueryHandler собирает данные чтения без изменения состояния.
public class GetMyProgressQueryHandler : IRequestHandler<GetMyProgressQuery, MyProgressDto>
{
    private readonly IProgressDbContext _context;

    public GetMyProgressQueryHandler(IProgressDbContext context)
    {
        _context = context;
    }

    public async Task<MyProgressDto> Handle(GetMyProgressQuery request, CancellationToken cancellationToken)
    {
        var progresses = await _context.LessonProgresses
            .Where(p => p.StudentId == request.StudentId && p.IsCompleted)
            .ToListAsync(cancellationToken);

        // Возвращаем сырой прогресс, а контроллер дополняет его структурой курса.
        return new MyProgressDto
        {
            Courses = progresses
                .GroupBy(p => p.LessonId)
                .Select(g => new CourseProgressDto
                {
                    CourseId = Guid.Empty, // enriched by controller
                    TotalLessons = 0,
                    CompletedLessons = g.Count(),
                    ProgressPercent = 0
                })
                .ToList()
        };
    }
}

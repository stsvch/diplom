// GetLessonByIdQueryHandler.cs

using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Lessons.Queries.GetLessonById;

// Тип class: ключевой элемент файла GetLessonByIdQueryHandler.cs.
public class GetLessonByIdQueryHandler : IRequestHandler<GetLessonByIdQuery, LessonDto?>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public GetLessonByIdQueryHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<LessonDto?> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        var lesson = await _context.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        return lesson is null ? null : _mapper.Map<LessonDto>(lesson);
    }
}

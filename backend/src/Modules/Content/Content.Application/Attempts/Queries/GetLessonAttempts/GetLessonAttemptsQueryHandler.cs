// GetLessonAttemptsQueryHandler.cs

using AutoMapper;
using Content.Application.DTOs;
using Content.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.Attempts.Queries.GetLessonAttempts;

// Тип class: ключевой элемент файла GetLessonAttemptsQueryHandler.cs.
public class GetLessonAttemptsQueryHandler : IRequestHandler<GetLessonAttemptsQuery, List<LessonBlockAttemptDto>>
{
    private readonly IContentDbContext _context;
    private readonly IMapper _mapper;

    public GetLessonAttemptsQueryHandler(IContentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<List<LessonBlockAttemptDto>> Handle(GetLessonAttemptsQuery request, CancellationToken cancellationToken)
    {
        var blockIds = await _context.LessonBlocks
            .AsNoTracking()
            .Where(b => b.LessonId == request.LessonId)
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        var query = _context.LessonBlockAttempts
            .AsNoTracking()
            .Where(a => blockIds.Contains(a.BlockId));

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        var attempts = await query.OrderByDescending(a => a.SubmittedAt).ToListAsync(cancellationToken);

        return _mapper.Map<List<LessonBlockAttemptDto>>(attempts);
    }
}

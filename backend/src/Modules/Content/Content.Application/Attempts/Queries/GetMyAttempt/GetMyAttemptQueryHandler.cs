// GetMyAttemptQueryHandler.cs

using AutoMapper;
using Content.Application.DTOs;
using Content.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.Attempts.Queries.GetMyAttempt;

// Тип class: ключевой элемент файла GetMyAttemptQueryHandler.cs.
public class GetMyAttemptQueryHandler : IRequestHandler<GetMyAttemptQuery, LessonBlockAttemptDto?>
{
    private readonly IContentDbContext _context;
    private readonly IMapper _mapper;

    public GetMyAttemptQueryHandler(IContentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<LessonBlockAttemptDto?> Handle(GetMyAttemptQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _context.LessonBlockAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.BlockId == request.BlockId && a.UserId == request.UserId, cancellationToken);

        return attempt is null ? null : _mapper.Map<LessonBlockAttemptDto>(attempt);
    }
}

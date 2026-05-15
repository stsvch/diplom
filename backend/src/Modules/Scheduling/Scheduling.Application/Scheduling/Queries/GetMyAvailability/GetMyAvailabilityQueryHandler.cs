// GetMyAvailabilityQueryHandler.cs

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.DTOs;
using Scheduling.Application.Interfaces;

namespace Scheduling.Application.Scheduling.Queries.GetMyAvailability;

/// <summary>
/// Обработчик CQRS-запроса GetMyAvailabilityQuery: читает данные, применяет фильтры и возвращает DTO/Result.
/// </summary>
public class GetMyAvailabilityQueryHandler : IRequestHandler<GetMyAvailabilityQuery, List<TeacherAvailabilityDto>>
{
    private readonly ISchedulingDbContext _context;
    private readonly IMapper _mapper;

    public GetMyAvailabilityQueryHandler(ISchedulingDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Основной сценарий handler-а: проверки, чтение/изменение данных и возврат результата.
    public async Task<List<TeacherAvailabilityDto>> Handle(GetMyAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var rules = await _context.TeacherAvailabilities
            .Where(a => a.TeacherId == request.TeacherId)
            .OrderByDescending(a => a.IsActive)
            .ThenBy(a => a.DayOfWeek)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<TeacherAvailabilityDto>>(rules);
    }
}

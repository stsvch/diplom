using AutoMapper;
using Calendar.Application.DTOs;
using Calendar.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Application.Calendar.Queries.GetMonthEvents;

public class GetMonthEventsQueryHandler : IRequestHandler<GetMonthEventsQuery, Result<List<CalendarEventDto>>>
{
    private readonly ICalendarDbContext _context;
    private readonly IMapper _mapper;

    public GetMonthEventsQueryHandler(ICalendarDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<CalendarEventDto>>> Handle(GetMonthEventsQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        var events = await _context.CalendarEvents
            .Where(e => (e.UserId == null || e.UserId == request.UserId)
                && e.EventDate >= startDate
                && e.EventDate < endDate)
            .OrderBy(e => e.EventDate)
            .ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<List<CalendarEventDto>>(events));
    }
}

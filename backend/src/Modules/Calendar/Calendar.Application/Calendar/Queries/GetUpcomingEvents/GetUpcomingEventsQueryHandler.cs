using AutoMapper;
using Calendar.Application.DTOs;
using Calendar.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Application.Calendar.Queries.GetUpcomingEvents;

public class GetUpcomingEventsQueryHandler : IRequestHandler<GetUpcomingEventsQuery, Result<List<CalendarEventDto>>>
{
    private readonly ICalendarDbContext _context;
    private readonly IMapper _mapper;

    public GetUpcomingEventsQueryHandler(ICalendarDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<CalendarEventDto>>> Handle(GetUpcomingEventsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow.Date;

        var events = await _context.CalendarEvents
            .Where(e => (e.UserId == null || e.UserId == request.UserId) && e.EventDate >= now)
            .OrderBy(e => e.EventDate)
            .Take(request.Count)
            .ToListAsync(cancellationToken);

        return Result.Success(_mapper.Map<List<CalendarEventDto>>(events));
    }
}

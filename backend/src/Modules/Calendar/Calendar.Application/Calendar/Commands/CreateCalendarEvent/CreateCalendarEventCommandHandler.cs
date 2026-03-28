using AutoMapper;
using Calendar.Application.DTOs;
using Calendar.Application.Interfaces;
using Calendar.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Calendar.Application.Calendar.Commands.CreateCalendarEvent;

public class CreateCalendarEventCommandHandler : IRequestHandler<CreateCalendarEventCommand, Result<CalendarEventDto>>
{
    private readonly ICalendarDbContext _context;
    private readonly IMapper _mapper;

    public CreateCalendarEventCommandHandler(ICalendarDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<CalendarEventDto>> Handle(CreateCalendarEventCommand request, CancellationToken cancellationToken)
    {
        var calendarEvent = new CalendarEvent
        {
            UserId = request.UserId,
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            EventDate = request.EventDate,
            EventTime = request.EventTime,
            Type = request.Type,
            SourceType = request.SourceType,
            SourceId = request.SourceId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CalendarEvents.Add(calendarEvent);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<CalendarEventDto>(calendarEvent);
        return Result.Success(dto);
    }
}

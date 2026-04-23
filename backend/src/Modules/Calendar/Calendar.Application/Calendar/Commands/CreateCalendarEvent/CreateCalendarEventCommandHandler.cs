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
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<CalendarEventDto>("Укажите название события.");
        if (request.Type != EduPlatform.Shared.Domain.Enums.CalendarEventType.Custom)
            return Result.Failure<CalendarEventDto>("Через этот API можно создавать только пользовательские события.");
        if (!string.IsNullOrWhiteSpace(request.SourceType) || request.SourceId.HasValue)
            return Result.Failure<CalendarEventDto>("Пользовательское событие не может быть связано с системным источником.");

        var calendarEvent = new CalendarEvent
        {
            UserId = request.UserId,
            CourseId = request.CourseId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            EventDate = DateTime.SpecifyKind(request.EventDate.Date, DateTimeKind.Utc),
            EventTime = string.IsNullOrWhiteSpace(request.EventTime) ? null : request.EventTime.Trim(),
            Type = request.Type,
            SourceType = null,
            SourceId = null,
            CreatedAt = DateTime.UtcNow
        };

        _context.CalendarEvents.Add(calendarEvent);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<CalendarEventDto>(calendarEvent);
        return Result.Success(dto);
    }
}

using Calendar.Application.Interfaces;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Application.Calendar.Commands.DeleteCalendarEvent;

public class DeleteCalendarEventCommandHandler : IRequestHandler<DeleteCalendarEventCommand, Result>
{
    private readonly ICalendarDbContext _context;

    public DeleteCalendarEventCommandHandler(ICalendarDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteCalendarEventCommand request, CancellationToken cancellationToken)
    {
        var calendarEvent = await _context.CalendarEvents
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (calendarEvent is null)
            return Result.Failure("Событие не найдено.");
        if (calendarEvent.UserId != request.RequesterId)
            return Result.Failure("Нет прав на удаление этого события.");
        if (calendarEvent.Type != CalendarEventType.Custom || calendarEvent.SourceId.HasValue || !string.IsNullOrWhiteSpace(calendarEvent.SourceType))
            return Result.Failure("Можно удалять только пользовательские события.");

        _context.CalendarEvents.Remove(calendarEvent);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

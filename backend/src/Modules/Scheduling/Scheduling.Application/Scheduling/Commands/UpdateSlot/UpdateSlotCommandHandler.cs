using AutoMapper;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.DTOs;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Commands.UpdateSlot;

public class UpdateSlotCommandHandler : IRequestHandler<UpdateSlotCommand, Result<ScheduleSlotDto>>
{
    private readonly ISchedulingDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICalendarEventPublisher _calendar;

    public UpdateSlotCommandHandler(ISchedulingDbContext context, IMapper mapper, ICalendarEventPublisher calendar)
    {
        _context = context;
        _mapper = mapper;
        _calendar = calendar;
    }

    public async Task<Result<ScheduleSlotDto>> Handle(UpdateSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (slot == null)
            return Result.Failure<ScheduleSlotDto>("Слот не найден.");

        if (slot.TeacherId != request.TeacherId)
            return Result.Failure<ScheduleSlotDto>("Нет доступа к этому слоту.");

        if (slot.Status == SlotStatus.Cancelled || slot.Status == SlotStatus.Completed)
            return Result.Failure<ScheduleSlotDto>("Нельзя изменить завершённый или отменённый слот.");

        if (request.Title != null) slot.Title = request.Title;
        if (request.Description != null) slot.Description = request.Description;
        if (request.StartTime.HasValue) slot.StartTime = request.StartTime.Value;
        if (request.EndTime.HasValue) slot.EndTime = request.EndTime.Value;
        if (request.MeetingLink != null) slot.MeetingLink = request.MeetingLink;
        if (request.MaxStudents.HasValue) slot.MaxStudents = request.MaxStudents.Value;

        await _context.SaveChangesAsync(cancellationToken);

        await _calendar.UpsertAsync(new CalendarEventUpsert(
            slot.TeacherId, slot.CourseId, slot.Title, slot.Description,
            DateTime.SpecifyKind(slot.StartTime.Date, DateTimeKind.Utc),
            slot.StartTime.ToString("HH:mm"),
            CalendarEventType.Workshop, "ScheduleSlot", slot.Id), cancellationToken);

        foreach (var booking in slot.Bookings.Where(b => b.Status != BookingStatus.Cancelled))
        {
            await _calendar.UpsertAsync(new CalendarEventUpsert(
                booking.StudentId, slot.CourseId, slot.Title, slot.Description,
                DateTime.SpecifyKind(slot.StartTime.Date, DateTimeKind.Utc),
                slot.StartTime.ToString("HH:mm"),
                CalendarEventType.Workshop, "ScheduleSlot", slot.Id), cancellationToken);
        }

        return Result.Success(_mapper.Map<ScheduleSlotDto>(slot));
    }
}

using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Commands.CancelBooking;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result<string>>
{
    private readonly ISchedulingDbContext _context;

    public CancelBookingCommandHandler(ISchedulingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var slot = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.SlotId, cancellationToken);

        if (slot == null)
            return Result.Failure<string>("Слот не найден.");

        var booking = slot.Bookings.FirstOrDefault(b => b.StudentId == request.StudentId && b.Status == BookingStatus.Booked);

        if (booking == null)
            return Result.Failure<string>("Запись не найдена.");

        booking.Status = BookingStatus.Cancelled;

        // Restore slot to Available if it was Booked
        if (slot.Status == SlotStatus.Booked)
        {
            slot.Status = SlotStatus.Available;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success("Запись отменена.");
    }
}

using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Commands.CancelSlot;

public class CancelSlotCommandHandler : IRequestHandler<CancelSlotCommand, Result<string>>
{
    private readonly ISchedulingDbContext _context;

    public CancelSlotCommandHandler(ISchedulingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CancelSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (slot == null)
            return Result.Failure<string>("Слот не найден.");

        if (slot.TeacherId != request.TeacherId)
            return Result.Failure<string>("Нет доступа к этому слоту.");

        if (slot.Status == SlotStatus.Cancelled)
            return Result.Failure<string>("Слот уже отменён.");

        slot.Status = SlotStatus.Cancelled;

        // Cancel all active bookings
        foreach (var booking in slot.Bookings.Where(b => b.Status == BookingStatus.Booked))
        {
            booking.Status = BookingStatus.Cancelled;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success("Слот отменён.");
    }
}

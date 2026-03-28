using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Commands.CompleteSlot;

public class CompleteSlotCommandHandler : IRequestHandler<CompleteSlotCommand, Result<string>>
{
    private readonly ISchedulingDbContext _context;

    public CompleteSlotCommandHandler(ISchedulingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CompleteSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (slot == null)
            return Result.Failure<string>("Слот не найден.");

        if (slot.TeacherId != request.TeacherId)
            return Result.Failure<string>("Нет доступа к этому слоту.");

        if (slot.Status == SlotStatus.Cancelled)
            return Result.Failure<string>("Нельзя завершить отменённый слот.");

        if (slot.Status == SlotStatus.Completed)
            return Result.Failure<string>("Слот уже завершён.");

        slot.Status = SlotStatus.Completed;

        foreach (var booking in slot.Bookings.Where(b => b.Status == BookingStatus.Booked))
        {
            booking.Status = BookingStatus.Completed;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success("Слот завершён.");
    }
}

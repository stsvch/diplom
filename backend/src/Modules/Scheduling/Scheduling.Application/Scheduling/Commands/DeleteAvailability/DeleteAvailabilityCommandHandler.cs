using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Commands.DeleteAvailability;

public class DeleteAvailabilityCommandHandler : IRequestHandler<DeleteAvailabilityCommand, Result<string>>
{
    private readonly ISchedulingDbContext _context;

    public DeleteAvailabilityCommandHandler(ISchedulingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(DeleteAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var availability = await _context.TeacherAvailabilities
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (availability == null)
            return Result.Failure<string>("Расписание не найдено.");
        if (availability.TeacherId != request.TeacherId)
            return Result.Failure<string>("Нет доступа к этому расписанию.");

        // Если есть будущие активные брони — soft-deactivate, чтобы не оборвать существующие записи.
        var hasFutureBookings = await _context.ScheduleSlots
            .AnyAsync(s => s.AvailabilityId == availability.Id
                        && s.StartTime > DateTime.UtcNow
                        && s.Bookings.Any(b => b.Status == BookingStatus.Booked),
                cancellationToken);

        if (hasFutureBookings)
        {
            availability.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success("Расписание скрыто. Уже забронированные занятия останутся в силе.");
        }

        _context.TeacherAvailabilities.Remove(availability);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success("Расписание удалено.");
    }
}

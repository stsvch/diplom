using Microsoft.EntityFrameworkCore;
using Scheduling.Domain.Entities;

namespace Scheduling.Application.Interfaces;

public interface ISchedulingDbContext
{
    DbSet<ScheduleSlot> ScheduleSlots { get; }
    DbSet<SessionBooking> SessionBookings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

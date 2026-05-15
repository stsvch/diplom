// ISchedulingDbContext.cs

using Microsoft.EntityFrameworkCore;
using Scheduling.Domain.Entities;

namespace Scheduling.Application.Interfaces;

/// <summary>
/// Контракт DbContext фиксирует DbSet-ы и сохранение изменений, доступные application-слою.
/// </summary>
public interface ISchedulingDbContext
{
    DbSet<ScheduleSlot> ScheduleSlots { get; }
    DbSet<SessionBooking> SessionBookings { get; }
    DbSet<TeacherAvailability> TeacherAvailabilities { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Application.Interfaces;

public interface ICalendarDbContext
{
    DbSet<CalendarEvent> CalendarEvents { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

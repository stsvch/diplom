// ICalendarDbContext.cs
using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Application.Interfaces;

// Контракт application-слоя отделяет бизнес-сценарии от конкретной инфраструктуры.
public interface ICalendarDbContext
{
    DbSet<CalendarEvent> CalendarEvents { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

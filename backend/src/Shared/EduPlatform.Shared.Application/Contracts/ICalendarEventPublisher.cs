// Файл: ICalendarEventPublisher.cs
using EduPlatform.Shared.Domain.Enums;

namespace EduPlatform.Shared.Application.Contracts;

// Межмодульный контракт CalendarEventUpsert передаёт минимальные данные между контекстами модулей.
public record CalendarEventUpsert(
    string? UserId,
    Guid? CourseId,
    string Title,
    string? Description,
    DateTime EventDate,
    string? EventTime,
    CalendarEventType Type,
    string SourceType,
    Guid SourceId);

// Интерфейс ICalendarEventPublisher задаёт контракт сервиса между слоями или модулями.
public interface ICalendarEventPublisher
{
    Task UpsertAsync(CalendarEventUpsert request, CancellationToken cancellationToken = default);

    Task UpsertManyAsync(IReadOnlyCollection<CalendarEventUpsert> requests, CancellationToken cancellationToken = default);

    Task DeleteBySourceAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken = default);

    Task DeleteBySourceForUserAsync(string sourceType, Guid sourceId, string userId, CancellationToken cancellationToken = default);

    Task DeleteByCourseForUserAsync(Guid courseId, string userId, CancellationToken cancellationToken = default);

    Task DeleteByCourseAsync(Guid courseId, CancellationToken cancellationToken = default);
}

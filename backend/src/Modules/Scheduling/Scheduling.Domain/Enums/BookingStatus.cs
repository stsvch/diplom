// BookingStatus.cs

namespace Scheduling.Domain.Enums;

/// <summary>
/// Тип BookingStatus относится к основному сценарию модуля и документирует его публичный контракт.
/// </summary>
public enum BookingStatus
{
    Booked,
    Completed,
    Cancelled,
    LateCancelled
}

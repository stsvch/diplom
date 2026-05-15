// SlotStatus.cs

namespace Scheduling.Domain.Enums;

/// <summary>
/// Тип SlotStatus относится к основному сценарию модуля и документирует его публичный контракт.
/// </summary>
public enum SlotStatus
{
    Available,
    Booked,
    Completed,
    Cancelled,
    Full
}

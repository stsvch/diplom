using EduPlatform.Shared.Domain;
using Scheduling.Domain.Enums;

namespace Scheduling.Domain.Entities;

public class SessionBooking : BaseEntity
{
    public Guid SlotId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;

    /// <summary>Денормализовано из слота для EXCLUDE-constraint на пересечения у студента.</summary>
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    public BookingStatus Status { get; set; } = BookingStatus.Booked;

    /// <summary>Ссылка на запись расхода квоты подписки. Заполняется при бронировании.</summary>
    public Guid? SubscriptionUsageId { get; set; }

    public ScheduleSlot Slot { get; set; } = null!;
}

using EduPlatform.Shared.Domain;
using Scheduling.Domain.Enums;

namespace Scheduling.Domain.Entities;

public class SessionBooking : BaseEntity
{
    public Guid SlotId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    public BookingStatus Status { get; set; } = BookingStatus.Booked;

    public ScheduleSlot Slot { get; set; } = null!;
}

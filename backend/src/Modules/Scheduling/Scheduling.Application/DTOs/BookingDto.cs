using Scheduling.Domain.Enums;

namespace Scheduling.Application.DTOs;

public class BookingDto
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; }
    public BookingStatus Status { get; set; }
}

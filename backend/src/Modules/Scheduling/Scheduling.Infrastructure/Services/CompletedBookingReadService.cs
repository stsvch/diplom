using EduPlatform.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace Scheduling.Infrastructure.Services;

public class CompletedBookingReadService : ICompletedBookingReadService
{
    private readonly ISchedulingDbContext _context;

    public CompletedBookingReadService(ISchedulingDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CompletedBookingInfo>> GetCompletedBetweenAsync(
        string studentId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        var rows = await _context.SessionBookings
            .Where(b => b.StudentId == studentId
                     && b.Status == BookingStatus.Completed
                     && b.StartTime >= periodStart
                     && b.StartTime < periodEnd)
            .Select(b => new
            {
                BookingId = b.Id,
                StudentId = b.StudentId,
                TeacherId = b.Slot.TeacherId,
                TeacherName = b.Slot.TeacherName,
                SessionType = b.Slot.SessionType,
                SessionStart = b.Slot.StartTime,
                CompletedAt = b.Slot.UpdatedAt ?? b.Slot.EndTime
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new CompletedBookingInfo(
                r.BookingId,
                r.StudentId,
                r.TeacherId,
                r.TeacherName,
                r.SessionType == SessionType.Individual ? LiveSlotKind.Individual : LiveSlotKind.Group,
                r.SessionStart,
                r.CompletedAt))
            .ToList();
    }
}

// GetTeachersWithScheduleQueryHandler.cs

using EduPlatform.Shared.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.DTOs;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Queries.GetTeachersWithSchedule;

/// <summary>
/// Обработчик CQRS-запроса GetTeachersWithScheduleQuery: читает данные, применяет фильтры и возвращает DTO/Result.
/// </summary>
public class GetTeachersWithScheduleQueryHandler : IRequestHandler<GetTeachersWithScheduleQuery, List<TeacherWithScheduleDto>>
{
    private readonly ISchedulingDbContext _context;
    private readonly IEnrollmentReadService _enrollment;

    public GetTeachersWithScheduleQueryHandler(
        ISchedulingDbContext context,
        IEnrollmentReadService enrollment)
    {
        _context = context;
        _enrollment = enrollment;
    }

    // Основной сценарий handler-а: проверки, чтение/изменение данных и возврат результата.
    public async Task<List<TeacherWithScheduleDto>> Handle(GetTeachersWithScheduleQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rules = await _context.TeacherAvailabilities
            .Where(a => a.IsActive
                     && (a.ValidUntil == null || a.ValidUntil >= today))
            .Select(a => new
            {
                a.TeacherId,
                a.TeacherName,
                a.SessionType,
                a.RequiredCourseId
            })
            .ToListAsync(cancellationToken);

        if (rules.Count == 0)
            return new List<TeacherWithScheduleDto>();

        var enrolled = string.IsNullOrEmpty(request.CurrentUserId)
            ? new HashSet<Guid>()
            : (await _enrollment.GetActiveCourseIdsForStudentAsync(request.CurrentUserId, cancellationToken)).ToHashSet();

        var visible = rules
            .Where(r => !r.RequiredCourseId.HasValue || enrolled.Contains(r.RequiredCourseId.Value))
            .ToList();

        return visible
            .GroupBy(r => new { r.TeacherId, r.TeacherName })
            .Select(g => new TeacherWithScheduleDto(
                g.Key.TeacherId,
                g.Key.TeacherName,
                g.Count(),
                g.Count(r => r.SessionType == SessionType.Individual),
                g.Count(r => r.SessionType == SessionType.Group)))
            .OrderBy(t => t.TeacherName)
            .ToList();
    }
}

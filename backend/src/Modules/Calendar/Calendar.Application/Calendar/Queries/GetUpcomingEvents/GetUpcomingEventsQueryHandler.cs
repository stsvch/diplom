using AutoMapper;
using Calendar.Application.DTOs;
using Calendar.Application.Interfaces;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using EduPlatform.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Application.Calendar.Queries.GetUpcomingEvents;

public class GetUpcomingEventsQueryHandler : IRequestHandler<GetUpcomingEventsQuery, Result<List<CalendarEventDto>>>
{
    private readonly ICalendarDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAssignmentReadService _assignmentRead;
    private readonly ITestReadService _testRead;

    public GetUpcomingEventsQueryHandler(
        ICalendarDbContext context,
        IMapper mapper,
        IAssignmentReadService assignmentRead,
        ITestReadService testRead)
    {
        _context = context;
        _mapper = mapper;
        _assignmentRead = assignmentRead;
        _testRead = testRead;
    }

    public async Task<Result<List<CalendarEventDto>>> Handle(GetUpcomingEventsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow.Date;

        var events = await _context.CalendarEvents
            .Where(e => (e.UserId == null || e.UserId == request.UserId) && e.EventDate >= now)
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.EventTime)
            .Take(request.Count)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<CalendarEventDto>>(events);
        await AttachStatusesAsync(dtos, request.UserId, cancellationToken);
        return Result.Success(dtos);
    }

    private async Task AttachStatusesAsync(List<CalendarEventDto> dtos, string userId, CancellationToken cancellationToken)
    {
        var assignmentIds = dtos
            .Where(d => d.SourceType == "Assignment" && d.SourceId.HasValue)
            .Select(d => d.SourceId!.Value)
            .Distinct()
            .ToList();

        var testIds = dtos
            .Where(d => d.SourceType == "Test" && d.SourceId.HasValue)
            .Select(d => d.SourceId!.Value)
            .Distinct()
            .ToList();

        var assignmentStatuses = assignmentIds.Count > 0
            ? await _assignmentRead.GetStatusesAsync(assignmentIds, userId, cancellationToken)
            : null;
        var testStatuses = testIds.Count > 0
            ? await _testRead.GetStatusesAsync(testIds, userId, cancellationToken)
            : null;

        foreach (var dto in dtos)
        {
            if (!dto.SourceId.HasValue) continue;
            if (dto.SourceType == "Assignment" && assignmentStatuses != null)
                dto.Status = assignmentStatuses.TryGetValue(dto.SourceId.Value, out var s) ? s : DeadlineStatus.Pending;
            else if (dto.SourceType == "Test" && testStatuses != null)
                dto.Status = testStatuses.TryGetValue(dto.SourceId.Value, out var s) ? s : DeadlineStatus.Pending;
        }
    }
}

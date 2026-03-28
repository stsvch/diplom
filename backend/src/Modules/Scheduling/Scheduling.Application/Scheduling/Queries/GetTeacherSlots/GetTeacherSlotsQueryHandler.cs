using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.DTOs;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Queries.GetTeacherSlots;

public class GetTeacherSlotsQueryHandler : IRequestHandler<GetTeacherSlotsQuery, List<ScheduleSlotDto>>
{
    private readonly ISchedulingDbContext _context;
    private readonly IMapper _mapper;

    public GetTeacherSlotsQueryHandler(ISchedulingDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ScheduleSlotDto>> Handle(GetTeacherSlotsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ScheduleSlots
            .Include(s => s.Bookings)
            .Where(s => s.TeacherId == request.TeacherId);

        if (request.Status.HasValue)
        {
            query = query.Where(s => s.Status == request.Status.Value);
        }

        var slots = await query.OrderBy(s => s.StartTime).ToListAsync(cancellationToken);
        return _mapper.Map<List<ScheduleSlotDto>>(slots);
    }
}

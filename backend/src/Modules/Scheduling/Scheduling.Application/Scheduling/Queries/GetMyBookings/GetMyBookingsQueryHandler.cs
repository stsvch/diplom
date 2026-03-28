using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.DTOs;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Queries.GetMyBookings;

public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, List<ScheduleSlotDto>>
{
    private readonly ISchedulingDbContext _context;
    private readonly IMapper _mapper;

    public GetMyBookingsQueryHandler(ISchedulingDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ScheduleSlotDto>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        var slots = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .Where(s => s.Bookings.Any(b => b.StudentId == request.StudentId && b.Status != BookingStatus.Cancelled))
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ScheduleSlotDto>>(slots);
    }
}

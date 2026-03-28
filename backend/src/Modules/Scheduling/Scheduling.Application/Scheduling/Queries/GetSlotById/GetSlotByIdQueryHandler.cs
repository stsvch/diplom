using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.DTOs;
using Scheduling.Application.Interfaces;

namespace Scheduling.Application.Scheduling.Queries.GetSlotById;

public class GetSlotByIdQueryHandler : IRequestHandler<GetSlotByIdQuery, Result<ScheduleSlotDto>>
{
    private readonly ISchedulingDbContext _context;
    private readonly IMapper _mapper;

    public GetSlotByIdQueryHandler(ISchedulingDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<ScheduleSlotDto>> Handle(GetSlotByIdQuery request, CancellationToken cancellationToken)
    {
        var slot = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (slot == null)
            return Result.Failure<ScheduleSlotDto>("Слот не найден.");

        return Result.Success(_mapper.Map<ScheduleSlotDto>(slot));
    }
}

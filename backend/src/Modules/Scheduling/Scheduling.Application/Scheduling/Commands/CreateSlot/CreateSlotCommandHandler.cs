using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.DTOs;
using Scheduling.Application.Interfaces;
using Scheduling.Domain.Entities;

namespace Scheduling.Application.Scheduling.Commands.CreateSlot;

public class CreateSlotCommandHandler : IRequestHandler<CreateSlotCommand, Result<ScheduleSlotDto>>
{
    private readonly ISchedulingDbContext _context;
    private readonly IMapper _mapper;

    public CreateSlotCommandHandler(ISchedulingDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<ScheduleSlotDto>> Handle(CreateSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = new ScheduleSlot
        {
            TeacherId = request.TeacherId,
            TeacherName = request.TeacherName,
            CourseId = request.CourseId,
            CourseName = request.CourseName,
            Title = request.Title,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsGroupSession = request.IsGroupSession,
            MaxStudents = request.MaxStudents,
            MeetingLink = request.MeetingLink,
        };

        _context.ScheduleSlots.Add(slot);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await _context.ScheduleSlots
            .Include(s => s.Bookings)
            .FirstAsync(s => s.Id == slot.Id, cancellationToken);

        var dto = _mapper.Map<ScheduleSlotDto>(created);
        return Result.Success(dto);
    }
}

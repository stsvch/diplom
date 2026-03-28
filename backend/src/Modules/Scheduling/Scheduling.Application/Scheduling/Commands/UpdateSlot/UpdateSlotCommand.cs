using EduPlatform.Shared.Domain;
using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Commands.UpdateSlot;

public record UpdateSlotCommand(
    Guid Id,
    string TeacherId,
    string? Title,
    string? Description,
    DateTime? StartTime,
    DateTime? EndTime,
    string? MeetingLink,
    int? MaxStudents
) : IRequest<Result<ScheduleSlotDto>>;

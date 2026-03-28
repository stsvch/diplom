using EduPlatform.Shared.Domain;
using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Commands.CreateSlot;

public record CreateSlotCommand(
    string TeacherId,
    string TeacherName,
    Guid? CourseId,
    string? CourseName,
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    bool IsGroupSession,
    int MaxStudents,
    string? MeetingLink
) : IRequest<Result<ScheduleSlotDto>>;

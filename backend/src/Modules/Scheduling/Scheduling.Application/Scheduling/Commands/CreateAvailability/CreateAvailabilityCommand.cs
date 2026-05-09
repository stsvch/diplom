using EduPlatform.Shared.Domain;
using MediatR;
using Scheduling.Application.DTOs;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Commands.CreateAvailability;

public record CreateAvailabilityCommand(
    string TeacherId,
    string TeacherName,
    AvailabilityKind Kind,
    DayOfWeek? DayOfWeek,
    DateOnly? SpecificDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    int BreakBetweenMinutes,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    SessionType SessionType,
    int MaxStudents,
    string Title,
    string? Description,
    string? MeetingLink,
    Guid? RequiredCourseId
) : IRequest<Result<TeacherAvailabilityDto>>;

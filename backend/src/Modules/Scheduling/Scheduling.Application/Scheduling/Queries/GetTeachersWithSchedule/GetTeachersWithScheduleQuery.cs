using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Queries.GetTeachersWithSchedule;

public record GetTeachersWithScheduleQuery(string? CurrentUserId) : IRequest<List<TeacherWithScheduleDto>>;

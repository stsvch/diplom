// GetTeachersWithScheduleQuery.cs

using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Queries.GetTeachersWithSchedule;

/// <summary>
/// CQRS-запрос GetTeachersWithScheduleQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetTeachersWithScheduleQuery(string? CurrentUserId) : IRequest<List<TeacherWithScheduleDto>>;

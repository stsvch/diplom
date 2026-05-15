// GetMyAvailabilityQuery.cs

using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Queries.GetMyAvailability;

/// <summary>
/// CQRS-запрос GetMyAvailabilityQuery описывает параметры чтения данных без изменения состояния.
/// </summary>
public record GetMyAvailabilityQuery(string TeacherId) : IRequest<List<TeacherAvailabilityDto>>;

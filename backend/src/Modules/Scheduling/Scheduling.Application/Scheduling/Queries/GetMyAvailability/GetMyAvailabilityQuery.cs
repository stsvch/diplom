using MediatR;
using Scheduling.Application.DTOs;

namespace Scheduling.Application.Scheduling.Queries.GetMyAvailability;

public record GetMyAvailabilityQuery(string TeacherId) : IRequest<List<TeacherAvailabilityDto>>;

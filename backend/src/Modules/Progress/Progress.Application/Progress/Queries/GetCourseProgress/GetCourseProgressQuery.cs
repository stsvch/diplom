using MediatR;
using Progress.Application.DTOs;

namespace Progress.Application.Progress.Queries.GetCourseProgress;

public record GetCourseProgressQuery(Guid CourseId, string StudentId, List<Guid> LessonIds) : IRequest<CourseProgressDto>;

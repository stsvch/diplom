using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.EnrollCourse;

public record EnrollCourseCommand(Guid CourseId, string StudentId) : IRequest<Result<string>>;

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.UnenrollCourse;

public record UnenrollCourseCommand(Guid CourseId, string StudentId) : IRequest<Result<string>>;

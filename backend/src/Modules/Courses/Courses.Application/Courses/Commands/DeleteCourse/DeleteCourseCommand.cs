using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.DeleteCourse;

public record DeleteCourseCommand(Guid Id, string TeacherId) : IRequest<Result<string>>;

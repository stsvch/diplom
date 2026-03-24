using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.PublishCourse;

public record PublishCourseCommand(Guid Id, string TeacherId) : IRequest<Result<string>>;

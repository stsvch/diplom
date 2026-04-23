using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.ForceArchiveCourse;

public record ForceArchiveCourseCommand(Guid Id, string AdminId, string Reason) : IRequest<Result<string>>;

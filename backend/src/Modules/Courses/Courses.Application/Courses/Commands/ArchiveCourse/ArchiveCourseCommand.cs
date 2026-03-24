using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.ArchiveCourse;

public record ArchiveCourseCommand(Guid Id, string TeacherId) : IRequest<Result<string>>;

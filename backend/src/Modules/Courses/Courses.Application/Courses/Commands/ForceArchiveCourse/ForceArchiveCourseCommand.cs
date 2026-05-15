// ForceArchiveCourseCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.ForceArchiveCourse;

// Тип record: ключевой элемент файла ForceArchiveCourseCommand.cs.
public record ForceArchiveCourseCommand(Guid Id, string AdminId, string Reason) : IRequest<Result<string>>;

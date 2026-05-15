// ArchiveCourseCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.ArchiveCourse;

// Тип record: ключевой элемент файла ArchiveCourseCommand.cs.
public record ArchiveCourseCommand(Guid Id, string TeacherId) : IRequest<Result<string>>;

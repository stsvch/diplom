// PublishCourseCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.PublishCourse;

// Тип record: ключевой элемент файла PublishCourseCommand.cs.
public record PublishCourseCommand(Guid Id, string TeacherId, bool Force = false) : IRequest<Result<PublishValidationResult>>;

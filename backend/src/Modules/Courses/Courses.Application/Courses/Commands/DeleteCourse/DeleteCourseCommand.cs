// DeleteCourseCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.DeleteCourse;

// Тип record: ключевой элемент файла DeleteCourseCommand.cs.
public record DeleteCourseCommand(Guid Id, string TeacherId) : IRequest<Result<string>>;

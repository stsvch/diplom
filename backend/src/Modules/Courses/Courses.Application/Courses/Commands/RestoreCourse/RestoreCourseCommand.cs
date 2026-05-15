// RestoreCourseCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.RestoreCourse;

// Тип record: ключевой элемент файла RestoreCourseCommand.cs.
public record RestoreCourseCommand(Guid Id, string TeacherId) : IRequest<Result<string>>;

// UnenrollCourseCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.UnenrollCourse;

// Тип record: ключевой элемент файла UnenrollCourseCommand.cs.
public record UnenrollCourseCommand(Guid CourseId, string StudentId) : IRequest<Result<string>>;

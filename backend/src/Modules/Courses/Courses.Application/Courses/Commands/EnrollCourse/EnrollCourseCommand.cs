// EnrollCourseCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.EnrollCourse;

// Тип record: ключевой элемент файла EnrollCourseCommand.cs.
public record EnrollCourseCommand(Guid CourseId, string StudentId, string StudentName) : IRequest<Result<string>>;

// UpdateCourseTagsCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.UpdateCourseTags;

// Тип record: ключевой элемент файла UpdateCourseTagsCommand.cs.
public record UpdateCourseTagsCommand(
    Guid CourseId,
    string TeacherId,
    List<string> Tags
) : IRequest<Result>;

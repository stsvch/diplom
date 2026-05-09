using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.UpdateCourseTags;

public record UpdateCourseTagsCommand(
    Guid CourseId,
    string TeacherId,
    List<string> Tags
) : IRequest<Result>;

using Courses.Application.DTOs;
using MediatR;

namespace Courses.Application.Lessons.Queries.GetModuleLessons;

public record GetModuleLessonsQuery(Guid ModuleId) : IRequest<List<LessonDto>>;

// GetModuleLessonsQuery.cs

using Courses.Application.DTOs;
using MediatR;

namespace Courses.Application.Lessons.Queries.GetModuleLessons;

// Тип record: ключевой элемент файла GetModuleLessonsQuery.cs.
public record GetModuleLessonsQuery(Guid ModuleId) : IRequest<List<LessonDto>>;

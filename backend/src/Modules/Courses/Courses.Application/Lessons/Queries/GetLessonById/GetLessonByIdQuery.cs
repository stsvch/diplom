// GetLessonByIdQuery.cs

using Courses.Application.DTOs;
using MediatR;

namespace Courses.Application.Lessons.Queries.GetLessonById;

// Тип record: ключевой элемент файла GetLessonByIdQuery.cs.
public record GetLessonByIdQuery(Guid Id) : IRequest<LessonDto?>;

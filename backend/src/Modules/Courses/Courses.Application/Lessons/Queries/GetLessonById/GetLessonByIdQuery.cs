using Courses.Application.DTOs;
using MediatR;

namespace Courses.Application.Lessons.Queries.GetLessonById;

public record GetLessonByIdQuery(Guid Id) : IRequest<LessonDto?>;

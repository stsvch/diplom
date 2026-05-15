// Файл: GetCourseProgressQuery.cs
using MediatR;
using Progress.Application.DTOs;

namespace Progress.Application.Progress.Queries.GetCourseProgress;

// Запрос GetCourseProgressQuery описывает параметры чтения для MediatR-обработчика.
public record GetCourseProgressQuery(Guid CourseId, string StudentId, List<Guid> LessonIds) : IRequest<CourseProgressDto>;

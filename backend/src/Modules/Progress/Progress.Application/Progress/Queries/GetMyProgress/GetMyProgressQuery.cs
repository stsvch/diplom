// Файл: GetMyProgressQuery.cs
using MediatR;
using Progress.Application.DTOs;

namespace Progress.Application.Progress.Queries.GetMyProgress;

// Запрос GetMyProgressQuery описывает параметры чтения для MediatR-обработчика.
public record GetMyProgressQuery(string StudentId) : IRequest<MyProgressDto>;

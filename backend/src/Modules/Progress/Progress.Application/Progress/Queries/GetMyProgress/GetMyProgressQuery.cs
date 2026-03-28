using MediatR;
using Progress.Application.DTOs;

namespace Progress.Application.Progress.Queries.GetMyProgress;

public record GetMyProgressQuery(string StudentId) : IRequest<MyProgressDto>;

// GradeResponseCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Tests.Application.Tests.Commands.GradeResponse;

/// <summary>
/// CQRS-команда GradeResponseCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record GradeResponseCommand(
    Guid ResponseId,
    string TeacherId,
    int Points,
    string? Comment
) : IRequest<Result<string>>;

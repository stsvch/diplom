// DeleteTestCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Tests.Application.Tests.Commands.DeleteTest;

/// <summary>
/// CQRS-команда DeleteTestCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record DeleteTestCommand(Guid Id, string CreatedById) : IRequest<Result<string>>;

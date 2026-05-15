// DeleteQuestionCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Tests.Application.Tests.Commands.DeleteQuestion;

/// <summary>
/// CQRS-команда DeleteQuestionCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record DeleteQuestionCommand(Guid Id, string CreatedById) : IRequest<Result<string>>;

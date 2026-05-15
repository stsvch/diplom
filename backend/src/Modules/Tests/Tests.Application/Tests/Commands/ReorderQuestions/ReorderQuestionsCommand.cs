// ReorderQuestionsCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Tests.Application.Tests.Commands.ReorderQuestions;

/// <summary>
/// CQRS-команда ReorderQuestionsCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record ReorderQuestionsCommand(
    Guid TestId,
    string CreatedById,
    List<Guid> OrderedIds
) : IRequest<Result<string>>;

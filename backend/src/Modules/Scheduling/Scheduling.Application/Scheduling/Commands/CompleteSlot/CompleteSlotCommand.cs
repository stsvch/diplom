// CompleteSlotCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Scheduling.Application.Scheduling.Commands.CompleteSlot;

/// <summary>
/// CQRS-команда CompleteSlotCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record CompleteSlotCommand(Guid Id, string TeacherId) : IRequest<Result<string>>;

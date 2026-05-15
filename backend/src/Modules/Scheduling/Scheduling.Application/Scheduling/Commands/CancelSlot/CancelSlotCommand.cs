// CancelSlotCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Scheduling.Application.Scheduling.Commands.CancelSlot;

/// <summary>
/// CQRS-команда CancelSlotCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record CancelSlotCommand(Guid Id, string TeacherId) : IRequest<Result<string>>;

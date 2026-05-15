// CancelBookingCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Scheduling.Application.Scheduling.Commands.CancelBooking;

/// <summary>
/// CQRS-команда CancelBookingCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record CancelBookingCommand(Guid SlotId, string StudentId) : IRequest<Result<string>>;

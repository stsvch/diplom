// BookSlotCommand.cs

using EduPlatform.Shared.Domain;
using MediatR;

namespace Scheduling.Application.Scheduling.Commands.BookSlot;

/// <summary>
/// CQRS-команда BookSlotCommand описывает входные данные операции, которая меняет состояние модуля.
/// </summary>
public record BookSlotCommand(
    Guid TeacherAvailabilityId,
    DateTime StartTime,
    string StudentId,
    string StudentName) : IRequest<Result<string>>;

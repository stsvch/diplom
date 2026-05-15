// DeleteCalendarEventCommand.cs
using EduPlatform.Shared.Domain;
using MediatR;

namespace Calendar.Application.Calendar.Commands.DeleteCalendarEvent;

// Command содержит входные данные операции, изменяющей состояние модуля.
public record DeleteCalendarEventCommand(Guid Id, string RequesterId) : IRequest<Result>;

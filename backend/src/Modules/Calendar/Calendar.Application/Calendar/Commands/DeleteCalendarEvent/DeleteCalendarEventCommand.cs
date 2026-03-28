using EduPlatform.Shared.Domain;
using MediatR;

namespace Calendar.Application.Calendar.Commands.DeleteCalendarEvent;

public record DeleteCalendarEventCommand(Guid Id, string RequesterId) : IRequest<Result>;

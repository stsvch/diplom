using EduPlatform.Shared.Domain;
using MediatR;

namespace Tests.Application.Tests.Commands.DeleteQuestion;

public record DeleteQuestionCommand(Guid Id, string CreatedById) : IRequest<Result<string>>;

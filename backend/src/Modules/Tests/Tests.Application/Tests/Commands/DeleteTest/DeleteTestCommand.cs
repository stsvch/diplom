using EduPlatform.Shared.Domain;
using MediatR;

namespace Tests.Application.Tests.Commands.DeleteTest;

public record DeleteTestCommand(Guid Id, string CreatedById) : IRequest<Result<string>>;

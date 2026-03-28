using EduPlatform.Shared.Domain;
using MediatR;

namespace Tests.Application.Tests.Commands.ReorderQuestions;

public record ReorderQuestionsCommand(
    Guid TestId,
    string CreatedById,
    List<Guid> OrderedIds
) : IRequest<Result<string>>;

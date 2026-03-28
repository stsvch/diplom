using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.Interfaces;

namespace Tests.Application.Tests.Commands.ReorderQuestions;

public class ReorderQuestionsCommandHandler : IRequestHandler<ReorderQuestionsCommand, Result<string>>
{
    private readonly ITestsDbContext _context;

    public ReorderQuestionsCommandHandler(ITestsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(ReorderQuestionsCommand request, CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .Include(t => t.Questions)
            .FirstOrDefaultAsync(t => t.Id == request.TestId, cancellationToken);

        if (test is null)
            return Result.Failure<string>("Тест не найден.");

        if (test.CreatedById != request.CreatedById)
            return Result.Failure<string>("Вы не являетесь автором этого теста.");

        if (request.OrderedIds.Count != test.Questions.Count)
            return Result.Failure<string>("Количество идентификаторов не совпадает с количеством вопросов.");

        var questionsDict = test.Questions.ToDictionary(q => q.Id);

        for (int i = 0; i < request.OrderedIds.Count; i++)
        {
            if (!questionsDict.TryGetValue(request.OrderedIds[i], out var question))
                return Result.Failure<string>($"Вопрос с Id {request.OrderedIds[i]} не найден в этом тесте.");

            question.OrderIndex = i;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Порядок вопросов обновлён.");
    }
}

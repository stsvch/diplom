using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;
using Tests.Domain.Entities;

namespace Tests.Application.Tests.Commands.AddQuestion;

public class AddQuestionCommandHandler : IRequestHandler<AddQuestionCommand, Result<QuestionDto>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;

    public AddQuestionCommandHandler(ITestsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<QuestionDto>> Handle(AddQuestionCommand request, CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .Include(t => t.Questions)
            .FirstOrDefaultAsync(t => t.Id == request.TestId, cancellationToken);

        if (test is null)
            return Result.Failure<QuestionDto>("Тест не найден.");

        if (test.CreatedById != request.CreatedById)
            return Result.Failure<QuestionDto>("Вы не являетесь автором этого теста.");

        var maxOrder = test.Questions.Any() ? test.Questions.Max(q => q.OrderIndex) : -1;

        var question = new Question
        {
            TestId = test.Id,
            Type = request.Type,
            Text = request.Text,
            Points = request.Points,
            OrderIndex = maxOrder + 1
        };

        for (int i = 0; i < request.AnswerOptions.Count; i++)
        {
            var opt = request.AnswerOptions[i];
            question.AnswerOptions.Add(new AnswerOption
            {
                QuestionId = question.Id,
                Text = opt.Text,
                IsCorrect = opt.IsCorrect,
                OrderIndex = i,
                MatchingPairValue = opt.MatchingPairValue
            });
        }

        _context.Questions.Add(question);

        test.MaxScore = test.Questions.Sum(q => q.Points) + request.Points;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<QuestionDto>(question);
        return Result.Success(dto);
    }
}

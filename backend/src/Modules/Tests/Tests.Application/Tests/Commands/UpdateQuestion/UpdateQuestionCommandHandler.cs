using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;
using Tests.Domain.Entities;

namespace Tests.Application.Tests.Commands.UpdateQuestion;

public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand, Result<QuestionDto>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;

    public UpdateQuestionCommandHandler(ITestsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<QuestionDto>> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .Include(q => q.Test)
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (question is null)
            return Result.Failure<QuestionDto>("Вопрос не найден.");

        if (question.Test.CreatedById != request.CreatedById)
            return Result.Failure<QuestionDto>("Вы не являетесь автором этого теста.");

        var oldPoints = question.Points;

        question.Type = request.Type;
        question.Text = request.Text;
        question.Points = request.Points;
        if (request.GradeType.HasValue)
            question.GradeType = request.GradeType.Value;
        question.Explanation = request.Explanation;
        question.ExpectedAnswer = request.ExpectedAnswer;

        // Remove old options
        _context.AnswerOptions.RemoveRange(question.AnswerOptions);

        // Add new options
        question.AnswerOptions.Clear();
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

        // Recalculate MaxScore
        var test = question.Test;
        var allQuestions = await _context.Questions
            .Where(q => q.TestId == test.Id)
            .ToListAsync(cancellationToken);
        test.MaxScore = allQuestions.Where(q => q.Id != question.Id).Sum(q => q.Points) + request.Points;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<QuestionDto>(question);
        return Result.Success(dto);
    }
}

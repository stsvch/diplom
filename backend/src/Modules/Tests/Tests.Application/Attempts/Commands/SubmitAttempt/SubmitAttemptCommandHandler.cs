using System.Text.Json;
using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;
using Tests.Domain.Enums;

namespace Tests.Application.Attempts.Commands.SubmitAttempt;

public class SubmitAttemptCommandHandler : IRequestHandler<SubmitAttemptCommand, Result<TestAttemptDetailDto>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;

    public SubmitAttemptCommandHandler(ITestsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TestAttemptDetailDto>> Handle(SubmitAttemptCommand request, CancellationToken cancellationToken)
    {
        var attempt = await _context.TestAttempts
            .Include(a => a.Test)
                .ThenInclude(t => t.Questions)
                    .ThenInclude(q => q.AnswerOptions)
            .Include(a => a.Responses)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken);

        if (attempt is null)
            return Result.Failure<TestAttemptDetailDto>("Попытка не найдена.");

        if (attempt.StudentId != request.StudentId)
            return Result.Failure<TestAttemptDetailDto>("Эта попытка не принадлежит вам.");

        if (attempt.Status != AttemptStatus.InProgress)
            return Result.Failure<TestAttemptDetailDto>("Попытка уже завершена.");

        var test = attempt.Test;
        var questions = test.Questions.ToDictionary(q => q.Id);
        var hasOpenAnswer = false;
        var totalScore = 0;

        foreach (var response in attempt.Responses)
        {
            if (!questions.TryGetValue(response.QuestionId, out var question))
                continue;

            switch (question.Type)
            {
                case QuestionType.SingleChoice:
                    GradeSingleChoice(response, question);
                    break;
                case QuestionType.MultipleChoice:
                    GradeMultipleChoice(response, question);
                    break;
                case QuestionType.TextInput:
                    GradeTextInput(response, question);
                    break;
                case QuestionType.Matching:
                    GradeMatching(response, question);
                    break;
                case QuestionType.OpenAnswer:
                    response.IsCorrect = null;
                    response.Points = null;
                    hasOpenAnswer = true;
                    break;
            }

            totalScore += response.Points ?? 0;
        }

        attempt.Score = totalScore;
        attempt.CompletedAt = DateTime.UtcNow;
        attempt.Status = hasOpenAnswer ? AttemptStatus.NeedsReview : AttemptStatus.Completed;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<TestAttemptDetailDto>(attempt);
        dto.Responses = _mapper.Map<List<TestResponseDto>>(attempt.Responses);

        if (test.ShowCorrectAnswers)
        {
            dto.Questions = _mapper.Map<List<QuestionDto>>(test.Questions.OrderBy(q => q.OrderIndex));
        }

        return Result.Success(dto);
    }

    private static void GradeSingleChoice(Domain.Entities.TestResponse response, Domain.Entities.Question question)
    {
        var selectedIds = DeserializeOptionIds(response.SelectedOptionIds);
        if (selectedIds == null || selectedIds.Count != 1)
        {
            response.IsCorrect = false;
            response.Points = 0;
            return;
        }

        var selectedId = selectedIds[0];
        var correctOption = question.AnswerOptions.FirstOrDefault(o => o.IsCorrect);
        response.IsCorrect = correctOption != null && correctOption.Id.ToString() == selectedId;
        response.Points = response.IsCorrect == true ? question.Points : 0;
    }

    private static void GradeMultipleChoice(Domain.Entities.TestResponse response, Domain.Entities.Question question)
    {
        var selectedIds = DeserializeOptionIds(response.SelectedOptionIds);
        var correctIds = question.AnswerOptions
            .Where(o => o.IsCorrect)
            .Select(o => o.Id.ToString())
            .OrderBy(id => id)
            .ToList();

        if (selectedIds == null || selectedIds.Count == 0)
        {
            response.IsCorrect = false;
            response.Points = 0;
            return;
        }

        var sortedSelected = selectedIds.OrderBy(id => id).ToList();
        response.IsCorrect = sortedSelected.SequenceEqual(correctIds);
        response.Points = response.IsCorrect == true ? question.Points : 0;
    }

    private static void GradeTextInput(Domain.Entities.TestResponse response, Domain.Entities.Question question)
    {
        if (string.IsNullOrWhiteSpace(response.TextAnswer))
        {
            response.IsCorrect = false;
            response.Points = 0;
            return;
        }

        var correctOption = question.AnswerOptions.FirstOrDefault(o => o.IsCorrect);
        if (correctOption == null)
        {
            response.IsCorrect = false;
            response.Points = 0;
            return;
        }

        response.IsCorrect = string.Equals(
            response.TextAnswer.Trim(),
            correctOption.Text.Trim(),
            StringComparison.OrdinalIgnoreCase);
        response.Points = response.IsCorrect == true ? question.Points : 0;
    }

    private static void GradeMatching(Domain.Entities.TestResponse response, Domain.Entities.Question question)
    {
        // For matching: SelectedOptionIds is a JSON array of strings like "leftId:rightValue"
        // We compare each option's Id with its MatchingPairValue
        var selectedIds = DeserializeOptionIds(response.SelectedOptionIds);
        if (selectedIds == null || selectedIds.Count == 0)
        {
            response.IsCorrect = false;
            response.Points = 0;
            return;
        }

        // Build expected mapping: optionId -> matchingPairValue
        var expected = question.AnswerOptions
            .Where(o => o.MatchingPairValue != null)
            .ToDictionary(o => o.Id.ToString(), o => o.MatchingPairValue!.Trim().ToLowerInvariant());

        // Parse student answers
        var studentMapping = new Dictionary<string, string>();
        foreach (var pair in selectedIds)
        {
            var parts = pair.Split(':', 2);
            if (parts.Length == 2)
            {
                studentMapping[parts[0]] = parts[1].Trim().ToLowerInvariant();
            }
        }

        var allCorrect = expected.Count > 0 && expected.All(kvp =>
            studentMapping.TryGetValue(kvp.Key, out var val) && val == kvp.Value);

        response.IsCorrect = allCorrect;
        response.Points = allCorrect ? question.Points : 0;
    }

    private static List<string>? DeserializeOptionIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json);
        }
        catch
        {
            return null;
        }
    }
}

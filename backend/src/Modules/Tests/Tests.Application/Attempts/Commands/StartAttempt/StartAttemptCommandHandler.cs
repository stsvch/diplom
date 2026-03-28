using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;
using Tests.Domain.Entities;
using Tests.Domain.Enums;

namespace Tests.Application.Attempts.Commands.StartAttempt;

public class StartAttemptCommandHandler : IRequestHandler<StartAttemptCommand, Result<TestAttemptStartDto>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;

    public StartAttemptCommandHandler(ITestsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TestAttemptStartDto>> Handle(StartAttemptCommand request, CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .Include(t => t.Questions.OrderBy(q => q.OrderIndex))
                .ThenInclude(q => q.AnswerOptions.OrderBy(a => a.OrderIndex))
            .FirstOrDefaultAsync(t => t.Id == request.TestId, cancellationToken);

        if (test is null)
            return Result.Failure<TestAttemptStartDto>("Тест не найден.");

        // Check deadline
        if (test.Deadline.HasValue && DateTime.UtcNow > test.Deadline.Value)
            return Result.Failure<TestAttemptStartDto>("Срок сдачи теста истёк.");

        // Check for existing InProgress attempt
        var existingInProgress = await _context.TestAttempts
            .AnyAsync(a => a.TestId == request.TestId
                        && a.StudentId == request.StudentId
                        && a.Status == AttemptStatus.InProgress, cancellationToken);

        if (existingInProgress)
            return Result.Failure<TestAttemptStartDto>("У вас уже есть незавершённая попытка.");

        // Check max attempts
        var completedAttempts = await _context.TestAttempts
            .CountAsync(a => a.TestId == request.TestId
                         && a.StudentId == request.StudentId, cancellationToken);

        if (test.MaxAttempts.HasValue && completedAttempts >= test.MaxAttempts.Value)
            return Result.Failure<TestAttemptStartDto>("Превышено максимальное количество попыток.");

        // Create attempt
        var attempt = new TestAttempt
        {
            TestId = test.Id,
            StudentId = request.StudentId,
            AttemptNumber = completedAttempts + 1,
            StartedAt = DateTime.UtcNow,
            Status = AttemptStatus.InProgress
        };

        _context.TestAttempts.Add(attempt);
        await _context.SaveChangesAsync(cancellationToken);

        // Prepare questions (without correct answers)
        var questions = test.Questions.ToList();

        // Shuffle questions if needed
        if (test.ShuffleQuestions)
        {
            var rng = new Random();
            questions = questions.OrderBy(_ => rng.Next()).ToList();
        }

        var studentQuestions = new List<StudentQuestionDto>();
        foreach (var q in questions)
        {
            var options = q.AnswerOptions.ToList();

            // Shuffle answers if needed
            if (test.ShuffleAnswers)
            {
                var rng = new Random();
                options = options.OrderBy(_ => rng.Next()).ToList();
            }

            studentQuestions.Add(new StudentQuestionDto
            {
                Id = q.Id,
                Type = q.Type,
                Text = q.Text,
                Points = q.Points,
                OrderIndex = q.OrderIndex,
                AnswerOptions = options.Select(o => new StudentAnswerOptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    OrderIndex = o.OrderIndex,
                    MatchingPairValue = o.MatchingPairValue
                }).ToList()
            });
        }

        return Result.Success(new TestAttemptStartDto
        {
            AttemptId = attempt.Id,
            Questions = studentQuestions,
            TimeLimitMinutes = test.TimeLimitMinutes,
            AttemptNumber = attempt.AttemptNumber
        });
    }
}

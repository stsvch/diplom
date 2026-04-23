using AutoMapper;
using Content.Application.DTOs;
using Content.Application.Interfaces;
using Content.Domain.Enums;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.Attempts.Commands.ReviewAttempt;

public class ReviewAttemptCommandHandler : IRequestHandler<ReviewAttemptCommand, Result<LessonBlockAttemptDto>>
{
    private readonly IContentDbContext _context;
    private readonly IMapper _mapper;

    public ReviewAttemptCommandHandler(IContentDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<LessonBlockAttemptDto>> Handle(ReviewAttemptCommand request, CancellationToken cancellationToken)
    {
        var attempt = await _context.LessonBlockAttempts.FindAsync([request.AttemptId], cancellationToken);
        if (attempt is null)
            return Result.Failure<LessonBlockAttemptDto>("Попытка не найдена.");

        if (request.Score < 0 || request.Score > attempt.MaxScore)
            return Result.Failure<LessonBlockAttemptDto>("Балл вне допустимого диапазона.");

        attempt.Score = request.Score;
        attempt.IsCorrect = request.Score == attempt.MaxScore;
        attempt.NeedsReview = false;
        attempt.Status = LessonBlockAttemptStatus.Graded;
        attempt.ReviewerId = request.ReviewerId;
        attempt.ReviewerComment = request.Comment;
        attempt.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<LessonBlockAttemptDto>(attempt));
    }
}

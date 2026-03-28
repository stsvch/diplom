using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;
using Tests.Domain.Enums;

namespace Tests.Application.Attempts.Queries.GetAttempt;

public class GetAttemptQueryHandler : IRequestHandler<GetAttemptQuery, Result<TestAttemptDetailDto>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;

    public GetAttemptQueryHandler(ITestsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TestAttemptDetailDto>> Handle(GetAttemptQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _context.TestAttempts
            .Include(a => a.Test)
                .ThenInclude(t => t.Questions.OrderBy(q => q.OrderIndex))
                    .ThenInclude(q => q.AnswerOptions.OrderBy(a => a.OrderIndex))
            .Include(a => a.Responses)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken);

        if (attempt is null)
            return Result.Failure<TestAttemptDetailDto>("Попытка не найдена.");

        var dto = _mapper.Map<TestAttemptDetailDto>(attempt);
        dto.Responses = _mapper.Map<List<TestResponseDto>>(attempt.Responses);

        // Show correct answers only if attempt is completed and ShowCorrectAnswers is enabled
        if (attempt.Status != AttemptStatus.InProgress && attempt.Test.ShowCorrectAnswers)
        {
            dto.Questions = _mapper.Map<List<QuestionDto>>(attempt.Test.Questions);
        }

        return Result.Success(dto);
    }
}

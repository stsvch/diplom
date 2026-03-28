using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;

namespace Tests.Application.Attempts.Queries.GetMyAttempts;

public class GetMyAttemptsQueryHandler : IRequestHandler<GetMyAttemptsQuery, Result<List<TestAttemptDto>>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;

    public GetMyAttemptsQueryHandler(ITestsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<TestAttemptDto>>> Handle(GetMyAttemptsQuery request, CancellationToken cancellationToken)
    {
        var testExists = await _context.Tests
            .AnyAsync(t => t.Id == request.TestId, cancellationToken);

        if (!testExists)
            return Result.Failure<List<TestAttemptDto>>("Тест не найден.");

        var attempts = await _context.TestAttempts
            .Include(a => a.Test)
            .Where(a => a.TestId == request.TestId && a.StudentId == request.StudentId)
            .OrderByDescending(a => a.StartedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<TestAttemptDto>>(attempts);
        return Result.Success(dtos);
    }
}

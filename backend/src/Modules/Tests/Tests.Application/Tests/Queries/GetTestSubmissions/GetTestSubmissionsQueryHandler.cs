using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;

namespace Tests.Application.Tests.Queries.GetTestSubmissions;

public class GetTestSubmissionsQueryHandler : IRequestHandler<GetTestSubmissionsQuery, Result<List<TestAttemptDto>>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;

    public GetTestSubmissionsQueryHandler(ITestsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<TestAttemptDto>>> Handle(GetTestSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TestId, cancellationToken);

        if (test is null)
            return Result.Failure<List<TestAttemptDto>>("Тест не найден.");

        if (test.CreatedById != request.CreatedById)
            return Result.Failure<List<TestAttemptDto>>("Вы не являетесь автором этого теста.");

        var attempts = await _context.TestAttempts
            .Include(a => a.Test)
            .Where(a => a.TestId == request.TestId)
            .OrderByDescending(a => a.StartedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<TestAttemptDto>>(attempts);
        return Result.Success(dtos);
    }
}

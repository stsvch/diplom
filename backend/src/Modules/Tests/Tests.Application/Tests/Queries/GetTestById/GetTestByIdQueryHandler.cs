using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;

namespace Tests.Application.Tests.Queries.GetTestById;

public class GetTestByIdQueryHandler : IRequestHandler<GetTestByIdQuery, Result<TestDetailDto>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;

    public GetTestByIdQueryHandler(ITestsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TestDetailDto>> Handle(GetTestByIdQuery request, CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .Include(t => t.Questions.OrderBy(q => q.OrderIndex))
                .ThenInclude(q => q.AnswerOptions.OrderBy(a => a.OrderIndex))
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (test is null)
            return Result.Failure<TestDetailDto>("Тест не найден.");

        var dto = _mapper.Map<TestDetailDto>(test);
        return Result.Success(dto);
    }
}

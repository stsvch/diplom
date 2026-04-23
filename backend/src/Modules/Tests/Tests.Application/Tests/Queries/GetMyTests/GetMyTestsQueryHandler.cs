using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;

namespace Tests.Application.Tests.Queries.GetMyTests;

public class GetMyTestsQueryHandler : IRequestHandler<GetMyTestsQuery, List<TestDto>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;

    public GetMyTestsQueryHandler(ITestsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<TestDto>> Handle(GetMyTestsQuery request, CancellationToken cancellationToken)
    {
        var tests = await _context.Tests
            .AsNoTracking()
            .Where(t => t.CreatedById == request.TeacherId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<TestDto>>(tests);
    }
}

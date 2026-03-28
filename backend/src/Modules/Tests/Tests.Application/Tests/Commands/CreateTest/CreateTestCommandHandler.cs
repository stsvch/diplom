using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;
using Tests.Domain.Entities;

namespace Tests.Application.Tests.Commands.CreateTest;

public class CreateTestCommandHandler : IRequestHandler<CreateTestCommand, Result<TestDetailDto>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;

    public CreateTestCommandHandler(ITestsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TestDetailDto>> Handle(CreateTestCommand request, CancellationToken cancellationToken)
    {
        var test = new Test
        {
            Title = request.Title,
            Description = request.Description,
            CreatedById = request.CreatedById,
            TimeLimitMinutes = request.TimeLimitMinutes,
            MaxAttempts = request.MaxAttempts,
            ShuffleQuestions = request.ShuffleQuestions,
            ShuffleAnswers = request.ShuffleAnswers,
            ShowCorrectAnswers = request.ShowCorrectAnswers,
            MaxScore = 0
        };

        _context.Tests.Add(test);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<TestDetailDto>(test);
        return Result.Success(dto);
    }
}

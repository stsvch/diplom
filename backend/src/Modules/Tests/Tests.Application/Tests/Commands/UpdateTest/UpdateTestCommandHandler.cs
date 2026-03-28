using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tests.Application.DTOs;
using Tests.Application.Interfaces;

namespace Tests.Application.Tests.Commands.UpdateTest;

public class UpdateTestCommandHandler : IRequestHandler<UpdateTestCommand, Result<TestDetailDto>>
{
    private readonly ITestsDbContext _context;
    private readonly IMapper _mapper;

    public UpdateTestCommandHandler(ITestsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TestDetailDto>> Handle(UpdateTestCommand request, CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .Include(t => t.Questions)
                .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (test is null)
            return Result.Failure<TestDetailDto>("Тест не найден.");

        if (test.CreatedById != request.CreatedById)
            return Result.Failure<TestDetailDto>("Вы не являетесь автором этого теста.");

        test.Title = request.Title;
        test.Description = request.Description;
        test.TimeLimitMinutes = request.TimeLimitMinutes;
        test.MaxAttempts = request.MaxAttempts;
        test.Deadline = request.Deadline;
        test.ShuffleQuestions = request.ShuffleQuestions;
        test.ShuffleAnswers = request.ShuffleAnswers;
        test.ShowCorrectAnswers = request.ShowCorrectAnswers;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<TestDetailDto>(test);
        return Result.Success(dto);
    }
}

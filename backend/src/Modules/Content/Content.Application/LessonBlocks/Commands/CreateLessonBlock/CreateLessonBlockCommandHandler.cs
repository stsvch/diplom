using AutoMapper;
using Content.Application.DTOs;
using Content.Application.Interfaces;
using Content.Application.Validation;
using Content.Domain.Entities;
using Content.Domain.ValueObjects.Blocks;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.LessonBlocks.Commands.CreateLessonBlock;

public class CreateLessonBlockCommandHandler : IRequestHandler<CreateLessonBlockCommand, Result<LessonBlockDto>>
{
    private readonly IContentDbContext _context;
    private readonly IMapper _mapper;
    private readonly IBlockDataValidatorRegistry _validator;

    public CreateLessonBlockCommandHandler(IContentDbContext context, IMapper mapper, IBlockDataValidatorRegistry validator)
    {
        _context = context;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<LessonBlockDto>> Handle(CreateLessonBlockCommand request, CancellationToken cancellationToken)
    {
        if (request.Data.Type != request.Type)
            return Result.Failure<LessonBlockDto>("Тип блока и тип данных не совпадают.");

        var validation = _validator.Validate(request.Type, request.Data);
        if (!validation.IsValid)
            return Result.Failure<LessonBlockDto>(string.Join("; ", validation.Errors));

        var maxOrder = await _context.LessonBlocks
            .Where(b => b.LessonId == request.LessonId)
            .MaxAsync(b => (int?)b.OrderIndex, cancellationToken) ?? -1;

        var block = new LessonBlock
        {
            LessonId = request.LessonId,
            Type = request.Type,
            Data = request.Data,
            Settings = request.Settings ?? new LessonBlockSettings(),
            OrderIndex = maxOrder + 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.LessonBlocks.Add(block);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<LessonBlockDto>(block));
    }
}

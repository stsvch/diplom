using AutoMapper;
using Content.Application.DTOs;
using Content.Application.Interfaces;
using Content.Application.Validation;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Content.Application.LessonBlocks.Commands.UpdateLessonBlock;

public class UpdateLessonBlockCommandHandler : IRequestHandler<UpdateLessonBlockCommand, Result<LessonBlockDto>>
{
    private readonly IContentDbContext _context;
    private readonly IMapper _mapper;
    private readonly IBlockDataValidatorRegistry _validator;

    public UpdateLessonBlockCommandHandler(IContentDbContext context, IMapper mapper, IBlockDataValidatorRegistry validator)
    {
        _context = context;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<LessonBlockDto>> Handle(UpdateLessonBlockCommand request, CancellationToken cancellationToken)
    {
        var block = await _context.LessonBlocks.FindAsync([request.Id], cancellationToken);
        if (block is null)
            return Result.Failure<LessonBlockDto>("Блок не найден.");

        if (request.Data.Type != block.Type)
            return Result.Failure<LessonBlockDto>("Тип данных не совпадает с типом блока.");

        var validation = _validator.Validate(block.Type, request.Data);
        if (!validation.IsValid)
            return Result.Failure<LessonBlockDto>(string.Join("; ", validation.Errors));

        block.Data = request.Data;
        if (request.Settings is not null)
            block.Settings = request.Settings;
        block.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<LessonBlockDto>(block));
    }
}

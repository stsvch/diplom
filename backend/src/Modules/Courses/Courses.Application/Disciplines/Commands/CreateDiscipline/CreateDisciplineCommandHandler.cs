// CreateDisciplineCommandHandler.cs

using AutoMapper;
using Courses.Application.DTOs;
using Courses.Domain.Entities;
using EduPlatform.Shared.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Disciplines.Commands.CreateDiscipline;

// Тип class: ключевой элемент файла CreateDisciplineCommandHandler.cs.
public class CreateDisciplineCommandHandler : IRequestHandler<CreateDisciplineCommand, Result<DisciplineDto>>
{
    private readonly IRepository<Discipline> _repository;
    private readonly IMapper _mapper;

    public CreateDisciplineCommandHandler(IRepository<Discipline> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<Result<DisciplineDto>> Handle(CreateDisciplineCommand request, CancellationToken cancellationToken)
    {
        var discipline = new Discipline
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl
        };

        await _repository.AddAsync(discipline, cancellationToken);
        return Result.Success(_mapper.Map<DisciplineDto>(discipline));
    }
}

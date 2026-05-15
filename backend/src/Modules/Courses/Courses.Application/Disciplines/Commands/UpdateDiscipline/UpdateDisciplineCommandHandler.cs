// UpdateDisciplineCommandHandler.cs

using AutoMapper;
using Courses.Application.DTOs;
using Courses.Domain.Entities;
using EduPlatform.Shared.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Disciplines.Commands.UpdateDiscipline;

// Тип class: ключевой элемент файла UpdateDisciplineCommandHandler.cs.
public class UpdateDisciplineCommandHandler : IRequestHandler<UpdateDisciplineCommand, Result<DisciplineDto>>
{
    private readonly IRepository<Discipline> _repository;
    private readonly IMapper _mapper;

    public UpdateDisciplineCommandHandler(IRepository<Discipline> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<Result<DisciplineDto>> Handle(UpdateDisciplineCommand request, CancellationToken cancellationToken)
    {
        var discipline = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (discipline == null)
            return Result.Failure<DisciplineDto>("Дисциплина не найдена.");

        discipline.Name = request.Name;
        discipline.Description = request.Description;
        discipline.ImageUrl = request.ImageUrl;

        await _repository.UpdateAsync(discipline, cancellationToken);
        return Result.Success(_mapper.Map<DisciplineDto>(discipline));
    }
}

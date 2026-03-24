using AutoMapper;
using Courses.Application.DTOs;
using Courses.Domain.Entities;
using EduPlatform.Shared.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Disciplines.Commands.CreateDiscipline;

public class CreateDisciplineCommandHandler : IRequestHandler<CreateDisciplineCommand, Result<DisciplineDto>>
{
    private readonly IRepository<Discipline> _repository;
    private readonly IMapper _mapper;

    public CreateDisciplineCommandHandler(IRepository<Discipline> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

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

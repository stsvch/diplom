using AutoMapper;
using Courses.Application.DTOs;
using Courses.Domain.Entities;
using EduPlatform.Shared.Application.Interfaces;
using MediatR;

namespace Courses.Application.Disciplines.Queries.GetDisciplines;

public class GetDisciplinesQueryHandler : IRequestHandler<GetDisciplinesQuery, List<DisciplineDto>>
{
    private readonly IRepository<Discipline> _repository;
    private readonly IMapper _mapper;

    public GetDisciplinesQueryHandler(IRepository<Discipline> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<DisciplineDto>> Handle(GetDisciplinesQuery request, CancellationToken cancellationToken)
    {
        var disciplines = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<DisciplineDto>>(disciplines);
    }
}

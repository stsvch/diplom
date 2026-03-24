using AutoMapper;
using Courses.Application.DTOs;
using Courses.Domain.Entities;
using EduPlatform.Shared.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Disciplines.Queries.GetDisciplineById;

public class GetDisciplineByIdQueryHandler : IRequestHandler<GetDisciplineByIdQuery, Result<DisciplineDto>>
{
    private readonly IRepository<Discipline> _repository;
    private readonly IMapper _mapper;

    public GetDisciplineByIdQueryHandler(IRepository<Discipline> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<DisciplineDto>> Handle(GetDisciplineByIdQuery request, CancellationToken cancellationToken)
    {
        var discipline = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (discipline == null)
            return Result.Failure<DisciplineDto>("Дисциплина не найдена.");

        return Result.Success(_mapper.Map<DisciplineDto>(discipline));
    }
}

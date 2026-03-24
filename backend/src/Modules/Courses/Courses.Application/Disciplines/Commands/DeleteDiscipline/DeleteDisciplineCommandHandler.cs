using Courses.Domain.Entities;
using EduPlatform.Shared.Application.Interfaces;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Disciplines.Commands.DeleteDiscipline;

public class DeleteDisciplineCommandHandler : IRequestHandler<DeleteDisciplineCommand, Result<string>>
{
    private readonly IRepository<Discipline> _repository;

    public DeleteDisciplineCommandHandler(IRepository<Discipline> repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(DeleteDisciplineCommand request, CancellationToken cancellationToken)
    {
        var discipline = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (discipline == null)
            return Result.Failure<string>("Дисциплина не найдена.");

        await _repository.DeleteAsync(discipline, cancellationToken);
        return Result.Success<string>("Дисциплина удалена.");
    }
}

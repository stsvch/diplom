// UnenrollCourseCommandHandler.cs

using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.UnenrollCourse;

// Тип class: ключевой элемент файла UnenrollCourseCommandHandler.cs.
public class UnenrollCourseCommandHandler : IRequestHandler<UnenrollCourseCommand, Result<string>>
{
    private readonly ICourseAccessRevocationService _courseAccessRevocationService;

    public UnenrollCourseCommandHandler(
        ICourseAccessRevocationService courseAccessRevocationService)
    {
        _courseAccessRevocationService = courseAccessRevocationService;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<Result<string>> Handle(UnenrollCourseCommand request, CancellationToken cancellationToken)
    {
        return await _courseAccessRevocationService.RevokeAccessAsync(
            request.CourseId,
            request.StudentId,
            cancellationToken);
    }
}

using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.UnenrollCourse;

public class UnenrollCourseCommandHandler : IRequestHandler<UnenrollCourseCommand, Result<string>>
{
    private readonly ICourseAccessRevocationService _courseAccessRevocationService;

    public UnenrollCourseCommandHandler(
        ICourseAccessRevocationService courseAccessRevocationService)
    {
        _courseAccessRevocationService = courseAccessRevocationService;
    }

    public async Task<Result<string>> Handle(UnenrollCourseCommand request, CancellationToken cancellationToken)
    {
        return await _courseAccessRevocationService.RevokeAccessAsync(
            request.CourseId,
            request.StudentId,
            cancellationToken);
    }
}

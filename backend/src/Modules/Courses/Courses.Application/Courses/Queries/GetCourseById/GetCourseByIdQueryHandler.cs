using Ardalis.Specification.EntityFrameworkCore;
using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using Courses.Application.Specifications;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Queries.GetCourseById;

public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, Result<CourseDetailDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public GetCourseByIdQueryHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<CourseDetailDto>> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new CourseByIdSpec(request.Id);
        var course = await _context.Courses
            .WithSpecification(spec)
            .FirstOrDefaultAsync(cancellationToken);

        if (course == null)
            return Result.Failure<CourseDetailDto>("Курс не найден.");

        var isAdmin = string.Equals(request.UserRole, "Admin", StringComparison.OrdinalIgnoreCase);
        var isCourseTeacher = !string.IsNullOrWhiteSpace(request.UserId)
            && course.TeacherId == request.UserId;

        if (course.IsArchived && !isAdmin && !isCourseTeacher)
            return Result.Failure<CourseDetailDto>("Курс не найден.");

        if (!course.IsPublished && !isAdmin && !isCourseTeacher)
            return Result.Failure<CourseDetailDto>("Курс не найден.");

        return Result.Success(_mapper.Map<CourseDetailDto>(course));
    }
}

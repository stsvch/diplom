using Ardalis.Specification.EntityFrameworkCore;
using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using Courses.Application.Specifications;
using Courses.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Queries.GetMyCourses;

public class GetMyCoursesQueryHandler : IRequestHandler<GetMyCoursesQuery, List<CourseListDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public GetMyCoursesQueryHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CourseListDto>> Handle(GetMyCoursesQuery request, CancellationToken cancellationToken)
    {
        if (request.Role.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
        {
            var spec = new TeacherCoursesSpec(request.UserId);
            var courses = await _context.Courses
                .WithSpecification(spec)
                .ToListAsync(cancellationToken);
            return _mapper.Map<List<CourseListDto>>(courses);
        }
        else
        {
            var spec = new StudentEnrolledCoursesSpec(request.UserId);
            var enrollments = await _context.CourseEnrollments
                .WithSpecification(spec)
                .ToListAsync(cancellationToken);
            return _mapper.Map<List<CourseListDto>>(enrollments.Select(e => e.Course).ToList());
        }
    }
}

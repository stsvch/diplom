using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using Courses.Application.Specifications;
using Courses.Domain.Entities;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Queries.GetCourseCatalog;

public class GetCourseCatalogQueryHandler : IRequestHandler<GetCourseCatalogQuery, PagedResult<CourseListDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public GetCourseCatalogQueryHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<CourseListDto>> Handle(GetCourseCatalogQuery request, CancellationToken cancellationToken)
    {
        var countSpec = new CourseCatalogCountSpec(request.DisciplineId, request.IsFree, request.Level, request.Search);
        var totalCount = await _context.Courses
            .WithSpecification(countSpec)
            .CountAsync(cancellationToken);

        var spec = new CourseCatalogSpec(request.DisciplineId, request.IsFree, request.Level, request.Search, request.SortBy, request.Page, request.PageSize);
        var courses = await _context.Courses
            .WithSpecification(spec)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<CourseListDto>>(courses);

        return new PagedResult<CourseListDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}

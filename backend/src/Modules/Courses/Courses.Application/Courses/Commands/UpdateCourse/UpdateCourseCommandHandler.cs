using Ardalis.Specification.EntityFrameworkCore;
using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using Courses.Application.Specifications;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Commands.UpdateCourse;

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, Result<CourseDetailDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public UpdateCourseCommandHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<CourseDetailDto>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var spec = new CourseByIdSpec(request.Id);
        var course = await _context.Courses
            .WithSpecification(spec)
            .FirstOrDefaultAsync(cancellationToken);

        if (course == null)
            return Result.Failure<CourseDetailDto>("Курс не найден.");

        if (course.TeacherId != request.TeacherId)
            return Result.Failure<CourseDetailDto>("Вы не можете редактировать чужой курс.");

        course.DisciplineId = request.DisciplineId;
        course.Title = request.Title;
        course.Description = request.Description;
        course.Price = request.Price;
        course.IsFree = request.IsFree;
        course.OrderType = request.OrderType;
        course.HasGrading = request.HasGrading;
        course.Level = request.Level;
        course.ImageUrl = request.ImageUrl;
        course.Tags = request.Tags;
        course.HasCertificate = request.HasCertificate;
        course.Deadline = request.Deadline;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<CourseDetailDto>(course));
    }
}

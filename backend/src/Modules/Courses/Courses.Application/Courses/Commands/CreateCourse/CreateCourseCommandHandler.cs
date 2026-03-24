using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using Courses.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.CreateCourse;

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Result<CourseDetailDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;

    public CreateCourseCommandHandler(ICoursesDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<CourseDetailDto>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var discipline = await _context.Disciplines.FindAsync([request.DisciplineId], cancellationToken);
        if (discipline == null)
            return Result.Failure<CourseDetailDto>("Дисциплина не найдена.");

        var course = new Course
        {
            DisciplineId = request.DisciplineId,
            TeacherId = request.TeacherId,
            TeacherName = request.TeacherName,
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            IsFree = request.IsFree,
            OrderType = request.OrderType,
            HasGrading = request.HasGrading,
            Level = request.Level,
            ImageUrl = request.ImageUrl,
            Tags = request.Tags,
            Discipline = discipline
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<CourseDetailDto>(course));
    }
}

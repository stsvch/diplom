using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using Courses.Domain.Entities;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Courses.Commands.CreateCourse;

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Result<CourseDetailDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;
    private readonly IChatAdmin _chatAdmin;

    public CreateCourseCommandHandler(ICoursesDbContext context, IMapper mapper, IChatAdmin chatAdmin)
    {
        _context = context;
        _mapper = mapper;
        _chatAdmin = chatAdmin;
    }

    public async Task<Result<CourseDetailDto>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var discipline = await _context.Disciplines.FindAsync([request.DisciplineId], cancellationToken);
        if (discipline == null)
            return Result.Failure<CourseDetailDto>("Дисциплина не найдена.");

        if (request.IsFree)
            request = request with { Price = null };
        else if (!request.Price.HasValue || request.Price.Value <= 0)
            return Result.Failure<CourseDetailDto>("Для платного курса цена должна быть больше 0.");

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
            HasCertificate = request.HasCertificate,
            Deadline = request.Deadline,
            Discipline = discipline
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync(cancellationToken);

        await _chatAdmin.CreateCourseChatAsync(
            course.Id.ToString(),
            course.Title,
            course.TeacherId,
            course.TeacherName,
            cancellationToken);

        return Result.Success(_mapper.Map<CourseDetailDto>(course));
    }
}

using AutoMapper;
using Courses.Application.DTOs;
using Courses.Domain.Entities;

namespace Courses.Application.Mappings;

public class CoursesMappingProfile : Profile
{
    public CoursesMappingProfile()
    {
        CreateMap<Discipline, DisciplineDto>()
            .ForMember(d => d.CourseCount, opt => opt.MapFrom(s => s.Courses.Count));

        CreateMap<Course, CourseListDto>()
            .ForMember(d => d.DisciplineName, opt => opt.MapFrom(s => s.Discipline != null ? s.Discipline.Name : string.Empty))
            .ForMember(d => d.StudentsCount, opt => opt.MapFrom(s => s.Enrollments.Count))
            .ForMember(d => d.LessonsCount, opt => opt.MapFrom(s => s.Modules.SelectMany(m => m.Lessons).Count()))
            .ForMember(d => d.Duration, opt => opt.MapFrom(s => s.Modules.SelectMany(m => m.Lessons).Sum(l => l.Duration ?? 0)))
            .ForMember(d => d.Rating, opt => opt.Ignore())
            .ForMember(d => d.Progress, opt => opt.Ignore());

        CreateMap<Course, CourseDetailDto>()
            .ForMember(d => d.DisciplineName, opt => opt.MapFrom(s => s.Discipline != null ? s.Discipline.Name : string.Empty))
            .ForMember(d => d.StudentsCount, opt => opt.MapFrom(s => s.Enrollments.Count))
            .ForMember(d => d.LessonsCount, opt => opt.MapFrom(s => s.Modules.SelectMany(m => m.Lessons).Count()))
            .ForMember(d => d.Duration, opt => opt.MapFrom(s => s.Modules.SelectMany(m => m.Lessons).Sum(l => l.Duration ?? 0)))
            .ForMember(d => d.Rating, opt => opt.Ignore())
            .ForMember(d => d.Progress, opt => opt.Ignore())
            .ForMember(d => d.Modules, opt => opt.MapFrom(s => s.Modules.OrderBy(m => m.OrderIndex)));

        CreateMap<CourseModule, CourseModuleDto>()
            .ForMember(d => d.LessonsCount, opt => opt.MapFrom(s => s.Lessons.Count))
            .ForMember(d => d.Lessons, opt => opt.MapFrom(s => s.Lessons.OrderBy(l => l.OrderIndex)));

        CreateMap<Lesson, LessonDto>()
            .ForMember(d => d.BlocksCount, opt => opt.MapFrom(s => s.Blocks.Count));

        CreateMap<LessonBlock, LessonBlockDto>();
    }
}

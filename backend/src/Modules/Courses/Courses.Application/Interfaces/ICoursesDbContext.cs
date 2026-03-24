using Courses.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Interfaces;

public interface ICoursesDbContext
{
    DbSet<Discipline> Disciplines { get; }
    DbSet<Course> Courses { get; }
    DbSet<CourseModule> CourseModules { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<LessonBlock> LessonBlocks { get; }
    DbSet<CourseEnrollment> CourseEnrollments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

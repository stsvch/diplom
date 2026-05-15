// ICoursesDbContext.cs

using Courses.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Interfaces;

// Контракт interface: задаёт границу между слоями без привязки application layer к инфраструктуре.
public interface ICoursesDbContext
{
    DbSet<Discipline> Disciplines { get; }
    DbSet<Course> Courses { get; }
    DbSet<CourseModule> CourseModules { get; }
    DbSet<CourseItem> CourseItems { get; }
    DbSet<CourseReview> CourseReviews { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<CourseEnrollment> CourseEnrollments { get; }
    DbSet<Tag> Tags { get; }
    DbSet<CourseTag> CourseTags { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

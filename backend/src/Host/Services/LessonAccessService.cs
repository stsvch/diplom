using Content.Application.Interfaces;
using Courses.Application.Interfaces;
using Courses.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Host.Services;

public class LessonAccessService
{
    private readonly IContentDbContext _contentDb;
    private readonly ICoursesDbContext _coursesDb;

    public LessonAccessService(IContentDbContext contentDb, ICoursesDbContext coursesDb)
    {
        _contentDb = contentDb;
        _coursesDb = coursesDb;
    }

    public async Task<bool> CanStudentAccessLessonAsync(Guid lessonId, string studentId, CancellationToken cancellationToken = default)
    {
        return await (
            from lesson in _coursesDb.Lessons.AsNoTracking()
            join module in _coursesDb.CourseModules.AsNoTracking() on lesson.ModuleId equals module.Id
            join course in _coursesDb.Courses.AsNoTracking() on module.CourseId equals course.Id
            join enrollment in _coursesDb.CourseEnrollments.AsNoTracking() on module.CourseId equals enrollment.CourseId
            where lesson.Id == lessonId
                && !course.IsArchived
                && enrollment.StudentId == studentId
                && enrollment.Status == EnrollmentStatus.Active
            select lesson.Id)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> CanStudentAccessCourseAsync(Guid courseId, string studentId, CancellationToken cancellationToken = default)
    {
        return await (
            from course in _coursesDb.Courses.AsNoTracking()
            join enrollment in _coursesDb.CourseEnrollments.AsNoTracking() on course.Id equals enrollment.CourseId
            where course.Id == courseId
                && !course.IsArchived
                && enrollment.StudentId == studentId
                && enrollment.Status == EnrollmentStatus.Active
            select course.Id)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> CanStudentAccessModuleAsync(Guid moduleId, string studentId, CancellationToken cancellationToken = default)
    {
        return await (
            from module in _coursesDb.CourseModules.AsNoTracking()
            join course in _coursesDb.Courses.AsNoTracking() on module.CourseId equals course.Id
            join enrollment in _coursesDb.CourseEnrollments.AsNoTracking() on course.Id equals enrollment.CourseId
            where module.Id == moduleId
                && !course.IsArchived
                && enrollment.StudentId == studentId
                && enrollment.Status == EnrollmentStatus.Active
            select module.Id)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> CanTeacherManageCourseAsync(Guid courseId, string teacherId, CancellationToken cancellationToken = default)
    {
        return await _coursesDb.Courses
            .AsNoTracking()
            .AnyAsync(
                course => course.Id == courseId
                    && !course.IsArchived
                    && course.TeacherId == teacherId,
                cancellationToken);
    }

    public async Task<bool> CanTeacherManageModuleAsync(Guid moduleId, string teacherId, CancellationToken cancellationToken = default)
    {
        return await (
            from module in _coursesDb.CourseModules.AsNoTracking()
            join course in _coursesDb.Courses.AsNoTracking() on module.CourseId equals course.Id
            where module.Id == moduleId
                && !course.IsArchived
                && course.TeacherId == teacherId
            select module.Id)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> CanTeacherManageLessonAsync(Guid lessonId, string teacherId, CancellationToken cancellationToken = default)
    {
        return await (
            from lesson in _coursesDb.Lessons.AsNoTracking()
            join module in _coursesDb.CourseModules.AsNoTracking() on lesson.ModuleId equals module.Id
            join course in _coursesDb.Courses.AsNoTracking() on module.CourseId equals course.Id
            where lesson.Id == lessonId
                && !course.IsArchived
                && course.TeacherId == teacherId
            select lesson.Id)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> CanStudentAccessBlockAsync(Guid blockId, string studentId, CancellationToken cancellationToken = default)
    {
        var lessonId = await _contentDb.LessonBlocks
            .AsNoTracking()
            .Where(b => b.Id == blockId)
            .Select(b => (Guid?)b.LessonId)
            .FirstOrDefaultAsync(cancellationToken);

        return lessonId.HasValue
            && await CanStudentAccessLessonAsync(lessonId.Value, studentId, cancellationToken);
    }

    public async Task<bool> CanTeacherManageBlockAsync(Guid blockId, string teacherId, CancellationToken cancellationToken = default)
    {
        var lessonId = await _contentDb.LessonBlocks
            .AsNoTracking()
            .Where(b => b.Id == blockId)
            .Select(b => (Guid?)b.LessonId)
            .FirstOrDefaultAsync(cancellationToken);

        return lessonId.HasValue
            && await CanTeacherManageLessonAsync(lessonId.Value, teacherId, cancellationToken);
    }

    public async Task<bool> CanStudentAccessAttemptAsync(Guid attemptId, string studentId, CancellationToken cancellationToken = default)
    {
        var info = await (
            from attempt in _contentDb.LessonBlockAttempts.AsNoTracking()
            join block in _contentDb.LessonBlocks.AsNoTracking() on attempt.BlockId equals block.Id
            where attempt.Id == attemptId
            select new { attempt.UserId, block.LessonId })
            .FirstOrDefaultAsync(cancellationToken);

        return info is not null
            && info.UserId.ToString() == studentId
            && await CanStudentAccessLessonAsync(info.LessonId, studentId, cancellationToken);
    }

    public async Task<bool> CanTeacherManageAttemptAsync(Guid attemptId, string teacherId, CancellationToken cancellationToken = default)
    {
        var lessonId = await (
            from attempt in _contentDb.LessonBlockAttempts.AsNoTracking()
            join block in _contentDb.LessonBlocks.AsNoTracking() on attempt.BlockId equals block.Id
            where attempt.Id == attemptId
            select (Guid?)block.LessonId)
            .FirstOrDefaultAsync(cancellationToken);

        return lessonId.HasValue
            && await CanTeacherManageLessonAsync(lessonId.Value, teacherId, cancellationToken);
    }
}

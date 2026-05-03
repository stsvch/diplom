using Courses.Domain.Entities;
using Courses.Domain.Enums;
using Courses.Infrastructure.Persistence;
using EduPlatform.Host.Models.Courses;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Host.Services;

public class CourseReviewService
{
    private readonly CoursesDbContext _coursesDb;

    public CourseReviewService(CoursesDbContext coursesDb)
    {
        _coursesDb = coursesDb;
    }

    public async Task<List<CourseReviewDto>> GetReviewsAsync(Guid courseId, CancellationToken cancellationToken)
    {
        return await _coursesDb.CourseReviews
            .AsNoTracking()
            .Where(r => r.CourseId == courseId)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .Select(r => new CourseReviewDto
            {
                Id = r.Id,
                CourseId = r.CourseId,
                StudentId = r.StudentId,
                StudentName = r.StudentName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error, CourseReviewDto? Review)> UpsertReviewAsync(
        Guid courseId,
        string studentId,
        string studentName,
        UpsertCourseReviewRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Rating is < 1 or > 5)
            return (false, "Оценка должна быть от 1 до 5.", null);

        if (!string.IsNullOrWhiteSpace(request.Comment) && request.Comment.Length > 5000)
            return (false, "Комментарий слишком длинный.", null);

        var canReview = await _coursesDb.CourseEnrollments
            .AnyAsync(e =>
                e.CourseId == courseId
                && e.StudentId == studentId
                && e.Status != EnrollmentStatus.Dropped,
                cancellationToken);

        if (!canReview)
            return (false, "Оставить отзыв может только студент курса.", null);

        var review = await _coursesDb.CourseReviews
            .FirstOrDefaultAsync(r => r.CourseId == courseId && r.StudentId == studentId, cancellationToken);

        if (review is null)
        {
            review = new CourseReview
            {
                CourseId = courseId,
                StudentId = studentId,
                StudentName = string.IsNullOrWhiteSpace(studentName) ? "Студент" : studentName
            };

            _coursesDb.CourseReviews.Add(review);
        }

        review.StudentName = string.IsNullOrWhiteSpace(studentName) ? review.StudentName : studentName;
        review.Rating = request.Rating;
        review.Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();

        await _coursesDb.SaveChangesAsync(cancellationToken);
        await RecalculateCourseRatingAsync(courseId, cancellationToken);

        return (true, null, ToDto(review));
    }

    public async Task<(bool Success, string? Error)> DeleteReviewAsync(
        Guid courseId,
        Guid reviewId,
        string userId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var review = await _coursesDb.CourseReviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.CourseId == courseId, cancellationToken);

        if (review is null)
            return (false, "Отзыв не найден.");

        if (!isAdmin && review.StudentId != userId)
            return (false, "Нельзя удалить чужой отзыв.");

        _coursesDb.CourseReviews.Remove(review);
        await _coursesDb.SaveChangesAsync(cancellationToken);
        await RecalculateCourseRatingAsync(courseId, cancellationToken);
        return (true, null);
    }

    private async Task RecalculateCourseRatingAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _coursesDb.Courses.FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);
        if (course is null)
            return;

        var stats = await _coursesDb.CourseReviews
            .Where(r => r.CourseId == courseId)
            .GroupBy(r => r.CourseId)
            .Select(g => new
            {
                Count = g.Count(),
                Average = g.Average(r => r.Rating)
            })
            .FirstOrDefaultAsync(cancellationToken);

        course.RatingCount = stats?.Count ?? 0;
        course.ReviewsCount = stats?.Count ?? 0;
        course.RatingAverage = stats is null ? null : Math.Round(stats.Average, 2);
        await _coursesDb.SaveChangesAsync(cancellationToken);
    }

    private static CourseReviewDto ToDto(CourseReview review)
    {
        return new CourseReviewDto
        {
            Id = review.Id,
            CourseId = review.CourseId,
            StudentId = review.StudentId,
            StudentName = review.StudentName,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }
}

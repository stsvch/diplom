using Courses.Domain.Enums;
using EduPlatform.Shared.Domain;

namespace Courses.Domain.Entities;

public class CourseEnrollment : BaseEntity
{
    public Guid CourseId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    public Course Course { get; set; } = null!;
}

using Assignments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assignments.Application.Interfaces;

public interface IAssignmentsDbContext
{
    DbSet<Assignment> Assignments { get; }
    DbSet<AssignmentSubmission> AssignmentSubmissions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

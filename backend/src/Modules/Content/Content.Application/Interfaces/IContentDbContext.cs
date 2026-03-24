using Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Content.Application.Interfaces;

public interface IContentDbContext
{
    DbSet<Attachment> Attachments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

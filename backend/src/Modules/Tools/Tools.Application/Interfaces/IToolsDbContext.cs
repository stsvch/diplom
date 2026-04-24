using Microsoft.EntityFrameworkCore;
using Tools.Domain.Entities;

namespace Tools.Application.Interfaces;

public interface IToolsDbContext
{
    DbSet<DictionaryWord> DictionaryWords { get; }
    DbSet<UserDictionaryProgress> UserDictionaryProgress { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

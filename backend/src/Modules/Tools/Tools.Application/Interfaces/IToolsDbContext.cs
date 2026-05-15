// Файл: IToolsDbContext.cs
using Microsoft.EntityFrameworkCore;
using Tools.Domain.Entities;

namespace Tools.Application.Interfaces;

// DbContext IToolsDbContext описывает EF Core-модель и границы хранения данных модуля.
public interface IToolsDbContext
{
    DbSet<DictionaryWord> DictionaryWords { get; }
    DbSet<UserDictionaryProgress> UserDictionaryProgress { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// IExportService.cs

using Grading.Application.DTOs;

namespace Grading.Application.Interfaces;

/// <summary>
/// Контракт сервиса экспорта задаёт способы выгрузки журнала оценок.
/// </summary>
public interface IExportService
{
    Task<byte[]> ExportToExcelAsync(GradebookDto gradebook, CancellationToken cancellationToken = default);
    Task<byte[]> ExportToPdfAsync(GradebookDto gradebook, CancellationToken cancellationToken = default);
}

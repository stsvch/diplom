using Grading.Application.DTOs;

namespace Grading.Application.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportToExcelAsync(GradebookDto gradebook, CancellationToken cancellationToken = default);
    Task<byte[]> ExportToPdfAsync(GradebookDto gradebook, CancellationToken cancellationToken = default);
}

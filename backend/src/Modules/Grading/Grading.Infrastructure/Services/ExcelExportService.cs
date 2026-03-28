using ClosedXML.Excel;
using Grading.Application.DTOs;
using Grading.Application.Interfaces;

namespace Grading.Infrastructure.Services;

public class ExcelExportService : IExportService
{
    public Task<byte[]> ExportToExcelAsync(GradebookDto gradebook, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Журнал оценок");

        // Collect all unique assignment titles
        var allTitles = gradebook.Students
            .SelectMany(s => s.Grades.Select(g => g.Title))
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        // Header row
        ws.Cell(1, 1).Value = "Студент";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;

        for (int i = 0; i < allTitles.Count; i++)
        {
            var cell = ws.Cell(1, i + 2);
            cell.Value = allTitles[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Alignment.WrapText = true;
        }

        var avgColIndex = allTitles.Count + 2;
        var avgHeader = ws.Cell(1, avgColIndex);
        avgHeader.Value = "Средний (%)";
        avgHeader.Style.Font.Bold = true;
        avgHeader.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Student rows
        for (int row = 0; row < gradebook.Students.Count; row++)
        {
            var student = gradebook.Students[row];
            var excelRow = row + 2;

            ws.Cell(excelRow, 1).Value = student.StudentName;

            for (int col = 0; col < allTitles.Count; col++)
            {
                var title = allTitles[col];
                var grade = student.Grades.FirstOrDefault(g => g.Title == title);
                var cell = ws.Cell(excelRow, col + 2);

                if (grade != null)
                {
                    var pct = grade.MaxScore > 0 ? (double)(grade.Score / grade.MaxScore * 100) : 0;
                    cell.Value = $"{grade.Score}/{grade.MaxScore}";

                    // Color coding
                    if (pct >= 90)
                        cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                    else if (pct >= 75)
                        cell.Style.Fill.BackgroundColor = XLColor.Yellow;
                    else if (pct >= 60)
                        cell.Style.Fill.BackgroundColor = XLColor.Orange;
                    else
                        cell.Style.Fill.BackgroundColor = XLColor.LightSalmon;
                }
                else
                {
                    cell.Value = "-";
                }
            }

            var avgCell = ws.Cell(excelRow, avgColIndex);
            avgCell.Value = (double)student.AverageScore;
            avgCell.Style.NumberFormat.Format = "0.00";
            avgCell.Style.Font.Bold = true;
        }

        // Average row at bottom
        var avgRow = gradebook.Students.Count + 2;
        ws.Cell(avgRow, 1).Value = "Средний балл";
        ws.Cell(avgRow, 1).Style.Font.Bold = true;
        ws.Cell(avgRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;

        for (int col = 0; col < allTitles.Count; col++)
        {
            var title = allTitles[col];
            var avgScore = gradebook.Students
                .Select(s => s.Grades.FirstOrDefault(g => g.Title == title))
                .Where(g => g != null)
                .Select(g => g!.MaxScore > 0 ? (double)(g.Score / g.MaxScore * 100) : 0)
                .DefaultIfEmpty(0)
                .Average();

            var cell = ws.Cell(avgRow, col + 2);
            cell.Value = Math.Round(avgScore, 2);
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> ExportToPdfAsync(GradebookDto gradebook, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use PdfExportService for PDF export.");
    }
}

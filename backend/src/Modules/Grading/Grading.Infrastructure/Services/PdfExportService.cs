using Grading.Application.DTOs;
using Grading.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Grading.Infrastructure.Services;

public class PdfExportService : IExportService
{
    public Task<byte[]> ExportToExcelAsync(GradebookDto gradebook, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use ExcelExportService for Excel export.");
    }

    public Task<byte[]> ExportToPdfAsync(GradebookDto gradebook, CancellationToken cancellationToken = default)
    {
        var allTitles = gradebook.Students
            .SelectMany(s => s.Grades.Select(g => g.Title))
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text($"Журнал оценок: {gradebook.CourseName}")
                        .FontSize(16).Bold();
                    col.Item().Text($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    // Define columns
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3); // Student name
                        foreach (var _ in allTitles)
                            cols.RelativeColumn(2);
                        cols.RelativeColumn(2); // Average
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(4)
                            .Text("Студент").Bold();
                        foreach (var title in allTitles)
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4)
                                .Text(title).Bold().FontSize(8);
                        }
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(4)
                            .Text("Средний (%)").Bold();
                    });

                    // Student rows
                    foreach (var student in gradebook.Students)
                    {
                        table.Cell().Padding(4).Text(student.StudentName);

                        foreach (var title in allTitles)
                        {
                            var grade = student.Grades.FirstOrDefault(g => g.Title == title);
                            if (grade != null)
                            {
                                var pct = grade.MaxScore > 0 ? (double)(grade.Score / grade.MaxScore * 100) : 0;
                                var bgColor = pct >= 90 ? Colors.Green.Lighten3
                                    : pct >= 75 ? Colors.Yellow.Lighten3
                                    : pct >= 60 ? Colors.Orange.Lighten3
                                    : Colors.Red.Lighten3;

                                table.Cell().Background(bgColor).Padding(4)
                                    .Text($"{grade.Score}/{grade.MaxScore}");
                            }
                            else
                            {
                                table.Cell().Padding(4).Text("-").FontColor(Colors.Grey.Medium);
                            }
                        }

                        table.Cell().Background(Colors.Grey.Lighten4).Padding(4)
                            .Text($"{student.AverageScore:F1}%").Bold();
                    }

                    // Average row
                    table.Cell().Background(Colors.Grey.Lighten2).Padding(4)
                        .Text("Средний балл").Bold();

                    foreach (var title in allTitles)
                    {
                        var avg = gradebook.Students
                            .Select(s => s.Grades.FirstOrDefault(g => g.Title == title))
                            .Where(g => g != null)
                            .Select(g => g!.MaxScore > 0 ? (double)(g.Score / g.MaxScore * 100) : 0)
                            .DefaultIfEmpty(0)
                            .Average();

                        table.Cell().Background(Colors.Grey.Lighten2).Padding(4)
                            .Text($"{avg:F1}%").Bold();
                    }

                    // Overall average
                    var overallAvg = gradebook.Students.Any()
                        ? gradebook.Students.Average(s => s.AverageScore)
                        : 0;

                    table.Cell().Background(Colors.Grey.Lighten2).Padding(4)
                        .Text($"{overallAvg:F1}%").Bold();
                });

                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Страница ");
                        x.CurrentPageNumber();
                        x.Span(" из ");
                        x.TotalPages();
                    });
            });
        });

        var bytes = document.GeneratePdf();
        return Task.FromResult(bytes);
    }
}

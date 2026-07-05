using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HrPortal.Reporting.Infrastructure.Export;

public static class ReportFileWriter
{
    public static (byte[] Content, string ContentType, string FileName) Write(
        string reportTitle,
        string fileBaseName,
        string format,
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<object?>> rows)
    {
        return format.ToLowerInvariant() switch
        {
            "csv" => (GenerateCsv(headers, rows), "text/csv", $"{fileBaseName}.csv"),
            "xlsx" => (GenerateXlsx(reportTitle, headers, rows), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileBaseName}.xlsx"),
            "pdf" => (GeneratePdf(reportTitle, headers, rows), "application/pdf", $"{fileBaseName}.pdf"),
            _ => throw new InvalidOperationException("Unsupported export format.")
        };
    }

    private static byte[] GenerateCsv(
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<object?>> rows)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        foreach (var header in headers)
            csv.WriteField(header);
        csv.NextRecord();

        foreach (var row in rows)
        {
            foreach (var value in row)
                csv.WriteField(FormatValue(value));
            csv.NextRecord();
        }

        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] GenerateXlsx(
        string sheetName,
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<object?>> rows)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        for (var i = 0; i < headers.Count; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        var rowIndex = 2;
        foreach (var row in rows)
        {
            for (var col = 0; col < row.Count; col++)
                worksheet.Cell(rowIndex, col + 1).Value = FormatValue(row[col]);
            rowIndex++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] GeneratePdf(
        string title,
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<object?>> rows)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header().Text(title).SemiBold().FontSize(16);
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        for (var i = 0; i < headers.Count; i++)
                            columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        foreach (var headerText in headers)
                            header.Cell().Text(headerText);
                    });

                    foreach (var row in rows)
                    {
                        foreach (var value in row)
                            table.Cell().Text(FormatValue(value));
                    }
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string FormatValue(object? value) =>
        value switch
        {
            null => string.Empty,
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
            bool boolean => boolean ? "Yes" : "No",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };
}

using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using HrPortal.Employees.Application;
using HrPortal.Projects.Application;
using HrPortal.Tasks.Application;
using HrPortal.TimeTracking.Application;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HrPortal.TimeTracking.Infrastructure.Export;

internal sealed class TimeEntryExportService : ITimeEntryExportService
{
    private readonly ITimeEntryRepository _repository;
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly IProjectLookup _projectLookup;
    private readonly ITaskLookup _taskLookup;
    private readonly TenantContext _tenantContext;

    public TimeEntryExportService(
        ITimeEntryRepository repository,
        ITimesheetRepository timesheetRepository,
        IEmployeeLookup employeeLookup,
        IProjectLookup projectLookup,
        ITaskLookup taskLookup,
        TenantContext tenantContext)
    {
        _repository = repository;
        _timesheetRepository = timesheetRepository;
        _employeeLookup = employeeLookup;
        _projectLookup = projectLookup;
        _taskLookup = taskLookup;
        _tenantContext = tenantContext;
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(
        ExportTimeEntriesQuery query,
        bool includeEmployeeName,
        CancellationToken cancellationToken = default)
    {
        var scopeResult = await TimeEntryReadScope.ResolveAsync(
            _tenantContext,
            _employeeLookup,
            query.EmployeeId,
            cancellationToken);

        if (!scopeResult.IsSuccess)
            throw new InvalidOperationException(scopeResult.Error);

        var filter = scopeResult.Value!;
        var effectiveQuery = filter.EmployeeId.HasValue
            ? query with { EmployeeId = filter.EmployeeId }
            : query;

        var entries = await _repository.GetForExportAsync(
            effectiveQuery,
            filter.AllowedEmployeeIds,
            cancellationToken);

        var approvedEntryIds = await _timesheetRepository.GetApprovedTimeEntryIdsAsync(
            effectiveQuery.EmployeeId,
            filter.AllowedEmployeeIds,
            effectiveQuery.FromDate,
            effectiveQuery.ToDate,
            cancellationToken);

        entries = entries.Where(e => approvedEntryIds.Contains(e.Id)).ToList();

        if (effectiveQuery.Month.HasValue && effectiveQuery.Year.HasValue && !effectiveQuery.FromDate.HasValue)
        {
            var from = new DateOnly(effectiveQuery.Year.Value, effectiveQuery.Month.Value, 1);
            var to = from.AddMonths(1).AddDays(-1);
            approvedEntryIds = await _timesheetRepository.GetApprovedTimeEntryIdsAsync(
                effectiveQuery.EmployeeId,
                filter.AllowedEmployeeIds,
                from,
                to,
                cancellationToken);
            entries = entries.Where(e => approvedEntryIds.Contains(e.Id)).ToList();
        }

        var rows = await MapRowsAsync(entries, includeEmployeeName, cancellationToken);

        return query.Format.ToLowerInvariant() switch
        {
            "csv" => (GenerateCsv(rows, includeEmployeeName), "text/csv", "time-entries.csv"),
            "xlsx" => (GenerateXlsx(rows, includeEmployeeName), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "time-entries.xlsx"),
            "pdf" => (GeneratePdf(rows, includeEmployeeName), "application/pdf", "time-entries.pdf"),
            _ => throw new InvalidOperationException("Unsupported export format.")
        };
    }

    internal static async Task<IReadOnlyList<TimeEntryExportRow>> MapRowsAsync(
        IReadOnlyList<TimeEntry> entries,
        bool includeEmployeeName,
        IEmployeeLookup employeeLookup,
        IProjectLookup projectLookup,
        ITaskLookup taskLookup,
        CancellationToken cancellationToken)
    {
        var rows = new List<TimeEntryExportRow>(entries.Count);

        foreach (var entry in entries)
        {
            string? employeeName = null;
            if (includeEmployeeName)
                employeeName = await employeeLookup.GetFullNameAsync(entry.EmployeeId, cancellationToken);

            var projectName = await projectLookup.GetNameAsync(entry.ProjectId, cancellationToken) ?? "Unknown";
            string? taskTitle = null;
            if (entry.TaskId.HasValue)
                taskTitle = await taskLookup.GetTitleAsync(entry.TaskId.Value, cancellationToken);

            rows.Add(new TimeEntryExportRow(
                DateOnly.FromDateTime(entry.StartTime),
                employeeName,
                projectName,
                taskTitle,
                Math.Round(entry.WorkedMinutes / 60m, 2),
                entry.Description,
                entry.Billable));
        }

        return rows;
    }

    private async Task<IReadOnlyList<TimeEntryExportRow>> MapRowsAsync(
        IReadOnlyList<TimeEntry> entries,
        bool includeEmployeeName,
        CancellationToken cancellationToken) =>
        await MapRowsAsync(entries, includeEmployeeName, _employeeLookup, _projectLookup, _taskLookup, cancellationToken);

    private static byte[] GenerateCsv(IReadOnlyList<TimeEntryExportRow> rows, bool includeEmployeeName)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        if (includeEmployeeName)
        {
            csv.WriteField("Date");
            csv.WriteField("Employee");
            csv.WriteField("Project");
            csv.WriteField("Task");
            csv.WriteField("Hours");
            csv.WriteField("Description");
            csv.WriteField("Billable");
            csv.NextRecord();

            foreach (var row in rows)
            {
                csv.WriteField(row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                csv.WriteField(row.EmployeeName);
                csv.WriteField(row.ProjectName);
                csv.WriteField(row.TaskTitle);
                csv.WriteField(row.Hours);
                csv.WriteField(row.Description);
                csv.WriteField(row.Billable);
                csv.NextRecord();
            }
        }
        else
        {
            csv.WriteField("Date");
            csv.WriteField("Project");
            csv.WriteField("Task");
            csv.WriteField("Hours");
            csv.WriteField("Description");
            csv.WriteField("Billable");
            csv.NextRecord();

            foreach (var row in rows)
            {
                csv.WriteField(row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                csv.WriteField(row.ProjectName);
                csv.WriteField(row.TaskTitle);
                csv.WriteField(row.Hours);
                csv.WriteField(row.Description);
                csv.WriteField(row.Billable);
                csv.NextRecord();
            }
        }

        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] GenerateXlsx(IReadOnlyList<TimeEntryExportRow> rows, bool includeEmployeeName)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Time Entries");

        var headers = includeEmployeeName
            ? new[] { "Date", "Employee", "Project", "Task", "Hours", "Description", "Billable" }
            : new[] { "Date", "Project", "Task", "Hours", "Description", "Billable" };

        for (var i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        var rowIndex = 2;
        foreach (var row in rows)
        {
            var col = 1;
            worksheet.Cell(rowIndex, col++).Value = row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (includeEmployeeName)
                worksheet.Cell(rowIndex, col++).Value = row.EmployeeName;
            worksheet.Cell(rowIndex, col++).Value = row.ProjectName;
            worksheet.Cell(rowIndex, col++).Value = row.TaskTitle;
            worksheet.Cell(rowIndex, col++).Value = row.Hours;
            worksheet.Cell(rowIndex, col++).Value = row.Description;
            worksheet.Cell(rowIndex, col++).Value = row.Billable;
            rowIndex++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] GeneratePdf(IReadOnlyList<TimeEntryExportRow> rows, bool includeEmployeeName)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header().Text("Time Entries Export").SemiBold().FontSize(16);
                page.Content().Table(table =>
                {
                    if (includeEmployeeName)
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Date");
                            header.Cell().Text("Employee");
                            header.Cell().Text("Project");
                            header.Cell().Text("Task");
                            header.Cell().Text("Hours");
                            header.Cell().Text("Description");
                            header.Cell().Text("Billable");
                        });

                        foreach (var row in rows)
                        {
                            table.Cell().Text(row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                            table.Cell().Text(row.EmployeeName ?? string.Empty);
                            table.Cell().Text(row.ProjectName);
                            table.Cell().Text(row.TaskTitle ?? string.Empty);
                            table.Cell().Text(row.Hours.ToString(CultureInfo.InvariantCulture));
                            table.Cell().Text(row.Description ?? string.Empty);
                            table.Cell().Text(row.Billable ? "Yes" : "No");
                        }
                    }
                    else
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Date");
                            header.Cell().Text("Project");
                            header.Cell().Text("Task");
                            header.Cell().Text("Hours");
                            header.Cell().Text("Description");
                            header.Cell().Text("Billable");
                        });

                        foreach (var row in rows)
                        {
                            table.Cell().Text(row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                            table.Cell().Text(row.ProjectName);
                            table.Cell().Text(row.TaskTitle ?? string.Empty);
                            table.Cell().Text(row.Hours.ToString(CultureInfo.InvariantCulture));
                            table.Cell().Text(row.Description ?? string.Empty);
                            table.Cell().Text(row.Billable ? "Yes" : "No");
                        }
                    }
                });
            });
        });

        return document.GeneratePdf();
    }
}

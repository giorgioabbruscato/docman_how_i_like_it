using HrPortal.Reporting.Infrastructure.Export;
using HrPortal.Tenancy;
using HrPortal.TimeTracking.Application;
using HrPortal.TimeTracking.Application.Dtos;

namespace HrPortal.Reporting.Application.Generators;

internal sealed class WorkedHoursReportGenerator : IReportGenerator
{
    private readonly ITimeEntryExportService _exportService;
    private readonly TenantContext _tenantContext;

    public WorkedHoursReportGenerator(
        ITimeEntryExportService exportService,
        TenantContext tenantContext)
    {
        _exportService = exportService;
        _tenantContext = tenantContext;
    }

    public string ReportType => "worked-hours";

    public async Task<(byte[] Content, string ContentType, string FileName)> GenerateAsync(
        ReportQueryParams query,
        ReportGenerateFilter scope,
        CancellationToken cancellationToken = default)
    {
        var exportQuery = new ExportTimeEntriesQuery(
            query.Format,
            scope.EmployeeId ?? query.EmployeeId,
            query.ProjectId,
            query.FromDate,
            query.ToDate);

        var includeEmployeeName = _tenantContext.HasPermission("report.generate:team")
            || _tenantContext.HasPermission("report.generate:tenant");

        var (content, contentType, fileName) = await _exportService.ExportAsync(
            exportQuery,
            includeEmployeeName,
            cancellationToken);

        var baseName = Path.GetFileNameWithoutExtension(fileName);
        return (content, contentType, $"{baseName}-report{Path.GetExtension(fileName)}");
    }
}

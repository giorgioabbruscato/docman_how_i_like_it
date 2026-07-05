using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.TimeTracking.Application.Queries;

public sealed class ExportTimeEntriesQueryHandler
{
    private readonly ITimeEntryExportService _exportService;
    private readonly TenantContext _tenantContext;

    public ExportTimeEntriesQueryHandler(
        ITimeEntryExportService exportService,
        TenantContext tenantContext)
    {
        _exportService = exportService;
        _tenantContext = tenantContext;
    }

    public async Task<Result<(byte[] Content, string ContentType, string FileName)>> HandleAsync(
        ExportTimeEntriesQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var includeEmployeeName = _tenantContext.HasPermission("time_entry.read:team")
                || _tenantContext.HasPermission("time_entry.read:tenant");

            var result = await _exportService.ExportAsync(query, includeEmployeeName, cancellationToken);
            return Result.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<(byte[], string, string)>(ex.Message, "NOT_FOUND");
        }
    }
}

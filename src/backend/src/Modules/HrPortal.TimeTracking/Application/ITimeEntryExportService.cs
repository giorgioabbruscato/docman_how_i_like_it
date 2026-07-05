using HrPortal.TimeTracking.Application.Dtos;

namespace HrPortal.TimeTracking.Application;

public interface ITimeEntryExportService
{
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(
        ExportTimeEntriesQuery query,
        bool includeEmployeeName,
        CancellationToken cancellationToken = default);
}

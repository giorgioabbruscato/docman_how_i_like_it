namespace HrPortal.Reporting.Application;

public interface IReportGenerator
{
    string ReportType { get; }

    Task<(byte[] Content, string ContentType, string FileName)> GenerateAsync(
        ReportQueryParams query,
        ReportGenerateFilter scope,
        CancellationToken cancellationToken = default);
}

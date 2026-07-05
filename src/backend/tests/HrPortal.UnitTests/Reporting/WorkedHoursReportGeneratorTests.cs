using HrPortal.Reporting.Application;
using HrPortal.Reporting.Application.Generators;
using HrPortal.Tenancy;
using HrPortal.TimeTracking.Application;
using HrPortal.TimeTracking.Application.Dtos;
using Moq;

namespace HrPortal.UnitTests.Reporting;

public sealed class WorkedHoursReportGeneratorTests
{
    [Theory]
    [InlineData("csv")]
    [InlineData("xlsx")]
    [InlineData("pdf")]
    public async Task GenerateAsync_ProducesNonEmptyBytes_ForEachFormat(string format)
    {
        var exportService = new Mock<ITimeEntryExportService>();
        exportService.Setup(s => s.ExportAsync(
                It.IsAny<ExportTimeEntriesQuery>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new byte[] { 1, 2, 3 }, "text/csv", "time-entries.csv"));

        var tenantContext = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with
        {
            Permissions = ["report.generate:self"]
        };

        var generator = new WorkedHoursReportGenerator(exportService.Object, tenantContext);
        var scope = new ReportGenerateFilter(null, Guid.NewGuid());
        var query = new ReportQueryParams(format);

        var (content, _, _) = await generator.GenerateAsync(query, scope, CancellationToken.None);

        content.Should().NotBeEmpty();
    }
}

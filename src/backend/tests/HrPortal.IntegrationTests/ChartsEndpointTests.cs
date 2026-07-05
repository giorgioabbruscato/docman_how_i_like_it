using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.Analytics.Application.Dtos;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class ChartsEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static string CurrentMonthFrom =>
        new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).ToString("yyyy-MM-dd");

    private static string CurrentMonthTo =>
        new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month))
            .ToString("yyyy-MM-dd");

    private static readonly string[] ChartPaths =
    [
        "/api/v1/analytics/charts/hours-by-project",
        "/api/v1/analytics/charts/hours-by-department",
        "/api/v1/analytics/charts/hours-by-employee",
        "/api/v1/analytics/charts/hours-by-month",
        "/api/v1/analytics/charts/attendance-trend",
        "/api/v1/analytics/charts/leave-trend",
        "/api/v1/analytics/charts/budget-consumption"
    ];

    public ChartsEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Theory]
    [MemberData(nameof(ChartPathCases))]
    public async Task ChartEndpoint_ReturnsLabelsAndDatasets(string path)
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.GetAsync($"{path}?fromDate={CurrentMonthFrom}&toDate={CurrentMonthTo}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var chart = await response.Content.ReadFromJsonAsync<ChartResponseDto>(JsonOptions);
        chart.Should().NotBeNull();
        chart!.Labels.Should().NotBeNull();
        chart.Datasets.Should().NotBeNull();
    }

    [Fact]
    public async Task ChartEndpoint_ReturnsEmptyArrays_WhenNoDataInRange()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.GetAsync(
            "/api/v1/analytics/charts/hours-by-project?fromDate=1990-01-01&toDate=1990-01-31");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var chart = await response.Content.ReadFromJsonAsync<ChartResponseDto>(JsonOptions);
        chart!.Labels.Should().BeEmpty();
        chart.Datasets.Should().BeEmpty();
    }

    [Fact]
    public async Task ChartEndpoint_ReturnsForbidden_ForEmployeeWithoutPermission()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync("/api/v1/analytics/charts/hours-by-project");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string> ChartPathCases()
    {
        var data = new TheoryData<string>();
        foreach (var path in ChartPaths)
            data.Add(path);

        return data;
    }
}

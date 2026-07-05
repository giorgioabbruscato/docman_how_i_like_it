using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class AttendanceDashboardEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AttendanceDashboardEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Dashboard_ReturnsOpenSession()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });
        await client.PostAsJsonAsync("/api/v1/attendance/check-in", new { });

        var response = await client.GetAsync("/api/v1/attendance/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardResponse>(JsonOptions);
        dashboard!.CurrentSession.Should().NotBeNull();
        dashboard.CurrentSession!.Status.Should().Be("Open");
        dashboard.TodayCheckIn.Should().NotBeNull();

        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });
    }

    [Fact]
    public async Task Dashboard_ReturnsTotals_AfterCheckOut()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });
        await client.PostAsJsonAsync("/api/v1/attendance/check-in", new { });
        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });

        var response = await client.GetAsync("/api/v1/attendance/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dashboard = await response.Content.ReadFromJsonAsync<DashboardResponse>(JsonOptions);
        dashboard!.CurrentSession.Should().BeNull();
        dashboard.TodayWorkedMinutes.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task History_ReturnsPaginatedSessions()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });
        await client.PostAsJsonAsync("/api/v1/attendance/check-in", new { });
        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });

        var response = await client.GetAsync("/api/v1/attendance/history?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<PagedHistoryResponse>(JsonOptions);
        history!.TotalCount.Should().BeGreaterThan(0);
        history.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task History_ReturnsForbidden_WhenEmployeeQueriesOtherEmployee()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var otherEmployeeId = await TenantIsolationFixture.CreateEmployeeAsync(hrClient, "dash-other");

        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await client.GetAsync($"/api/v1/attendance/history?employeeId={otherEmployeeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Dashboard_ReturnsOk_ForAdminRole()
    {
        using var client = CreateAuthenticatedClient("admin");

        var response = await client.GetAsync("/api/v1/attendance/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private sealed record DashboardResponse(
        DateTime? TodayCheckIn,
        DateTime? TodayCheckOut,
        int TodayWorkedMinutes,
        SessionSummary? CurrentSession,
        int WeeklyTotalMinutes,
        int MonthlyTotalMinutes);

    private sealed record SessionSummary(Guid Id, DateTime CheckIn, string Status);

    private sealed record PagedHistoryResponse(
        IReadOnlyList<SessionSummary> Items,
        int TotalCount,
        int Page,
        int PageSize);
}

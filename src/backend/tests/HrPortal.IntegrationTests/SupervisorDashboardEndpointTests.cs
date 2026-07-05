using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Domain;
using HrPortal.Analytics.Application.Dtos;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class SupervisorDashboardEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SupervisorDashboardEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetSummary_ReturnsUnauthorized_WhenAnonymous()
    {
        using var client = CreateClient(includeTenantHeader: true);

        var response = await client.GetAsync("/api/v1/analytics/supervisor/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSummary_ReturnsForbidden_ForEmployeeWithoutPermission()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync("/api/v1/analytics/supervisor/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSummary_ReturnsAllWidgetSections_ForHrOnEnterpriseTenant()
    {
        using var client = CreateAuthenticatedClient("hr");
        await SeedAnalyticsDataAsync(client);

        var response = await client.GetAsync(
            $"/api/v1/analytics/supervisor/summary?fromDate={CurrentMonthFrom}&toDate={CurrentMonthTo}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<SupervisorSummaryDto>(JsonOptions);
        summary.Should().NotBeNull();
        summary!.EmployeesWorking.Should().NotBeNull();
        summary.AttendanceToday.Should().NotBeNull();
        summary.TopEmployees.Should().NotBeNull();
        summary.TopProjects.Should().NotBeNull();
        summary.BudgetUsage.Should().NotBeNull();
        summary.LateArrivals.Should().NotBeNull();
        summary.Overtime.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBudgetUsage_ReturnsForbidden_ForManager()
    {
        using var client = CreateAuthenticatedClient("manager");

        var response = await client.GetAsync("/api/v1/analytics/supervisor/budget-usage");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetBudgetUsage_ReturnsOk_ForHr()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.GetAsync("/api/v1/analytics/supervisor/budget-usage");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTopEmployees_RespectsDateFilter()
    {
        using var client = CreateAuthenticatedClient("hr");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client);
        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(client);
        var employeeId = await TenantIsolationFixture.CreateEmployeeInDepartmentAsync(client, departmentId, "analytics-top");
        await CreateTimeEntryForEmployeeAsync(client, employeeId, projectId, departmentId);

        var inRange = await client.GetAsync(
            $"/api/v1/analytics/supervisor/top-employees?fromDate={CurrentMonthFrom}&toDate={CurrentMonthTo}");
        inRange.StatusCode.Should().Be(HttpStatusCode.OK);
        var inRangeBody = await inRange.Content.ReadFromJsonAsync<List<TopEmployeeDto>>(JsonOptions);
        inRangeBody!.Should().NotBeEmpty();

        var outOfRange = await client.GetAsync(
            "/api/v1/analytics/supervisor/top-employees?fromDate=1990-01-01&toDate=1990-01-31");
        outOfRange.StatusCode.Should().Be(HttpStatusCode.OK);
        var outOfRangeBody = await outOfRange.Content.ReadFromJsonAsync<List<TopEmployeeDto>>(JsonOptions);
        outOfRangeBody!.Should().BeEmpty();
    }

    private static string CurrentMonthFrom =>
        new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).ToString("yyyy-MM-dd");

    private static string CurrentMonthTo =>
        new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month))
            .ToString("yyyy-MM-dd");

    private static async Task SeedAnalyticsDataAsync(HttpClient hrClient)
    {
        var projectId = await TenantIsolationFixture.CreateProjectAsync(hrClient, "analytics");
        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(hrClient);
        var employeeId = await TenantIsolationFixture.CreateEmployeeInDepartmentAsync(hrClient, departmentId, "analytics-seed");
        await CreateTimeEntryForEmployeeAsync(hrClient, employeeId, projectId, departmentId);
        await TenantIsolationFixture.CheckInAsync(hrClient, employeeId);
    }

    private static async Task CreateTimeEntryForEmployeeAsync(
        HttpClient hrClient,
        Guid employeeId,
        Guid projectId,
        Guid departmentId,
        int hoursAgo = 1)
    {
        var userId = Guid.NewGuid();
        var roleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(hrClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(hrClient, userId, roleId, employeeId);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/time-entries")
        {
            Content = System.Net.Http.Json.JsonContent.Create(new
            {
                projectId,
                startTime = DateTime.UtcNow.AddHours(-hoursAgo - 1),
                endTime = DateTime.UtcNow.AddHours(-hoursAgo)
            })
        };
        request.Headers.Add("X-Tenant-Id", hrClient.DefaultRequestHeaders.GetValues("X-Tenant-Id").First());
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "guest");
        request.Headers.Add(TestAuthHandler.UserIdHeaderName, userId.ToString());

        var response = await hrClient.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await ApproveTimesheetForEmployeeAsync(hrClient, userId, employeeId, departmentId);
    }

    private static async Task ApproveTimesheetForEmployeeAsync(
        HttpClient hrClient,
        Guid employeeUserId,
        Guid employeeId,
        Guid departmentId)
    {
        var from = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/timesheets")
        {
            Content = System.Net.Http.Json.JsonContent.Create(new { periodStart = from.ToString("yyyy-MM-dd"), periodEnd = to.ToString("yyyy-MM-dd") })
        };
        createRequest.Headers.Add("X-Tenant-Id", hrClient.DefaultRequestHeaders.GetValues("X-Tenant-Id").First());
        createRequest.Headers.Add(TestAuthHandler.RoleHeaderName, "guest");
        createRequest.Headers.Add(TestAuthHandler.UserIdHeaderName, employeeUserId.ToString());

        var createResponse = await hrClient.SendAsync(createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var timesheet = await createResponse.Content.ReadFromJsonAsync<TimesheetIdResponse>(JsonOptions);

        using var submitRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/timesheets/{timesheet!.Id}/submit");
        submitRequest.Headers.Add("X-Tenant-Id", hrClient.DefaultRequestHeaders.GetValues("X-Tenant-Id").First());
        submitRequest.Headers.Add(TestAuthHandler.RoleHeaderName, "guest");
        submitRequest.Headers.Add(TestAuthHandler.UserIdHeaderName, employeeUserId.ToString());
        (await hrClient.SendAsync(submitRequest)).StatusCode.Should().Be(HttpStatusCode.OK);

        var managerUserId = Guid.NewGuid();
        var managerRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.ManagerSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient,
            managerUserId,
            managerRoleId,
            attributes: new Dictionary<string, string> { ["departmentId"] = departmentId.ToString() });

        using var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/timesheets/{timesheet.Id}/approve");
        approveRequest.Headers.Add("X-Tenant-Id", hrClient.DefaultRequestHeaders.GetValues("X-Tenant-Id").First());
        approveRequest.Headers.Add(TestAuthHandler.RoleHeaderName, "guest");
        approveRequest.Headers.Add(TestAuthHandler.UserIdHeaderName, managerUserId.ToString());
        var approveResponse = await hrClient.SendAsync(approveRequest);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed record TimesheetIdResponse(Guid Id);
}

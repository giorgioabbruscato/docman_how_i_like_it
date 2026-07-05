using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class TimesheetApprovalEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TimesheetApprovalEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Create_ReturnsCreated_ForEmployee()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await client.PostAsJsonAsync("/api/v1/timesheets", new
        {
            periodStart = "2026-01-01",
            periodEnd = "2026-01-07"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Approve_ReturnsForbidden_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var create = await client.PostAsJsonAsync("/api/v1/timesheets", new
        {
            periodStart = "2026-02-01",
            periodEnd = "2026-02-07"
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var timesheet = await create.Content.ReadFromJsonAsync<TimesheetResponse>(JsonOptions);

        await client.PostAsync($"/api/v1/timesheets/{timesheet!.Id}/submit", null);

        var approve = await client.PostAsync($"/api/v1/timesheets/{timesheet.Id}/approve", null);
        approve.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Approve_ReturnsOk_ForManagerInSameDepartment()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(hrClient);
        var employeeId = await TenantIsolationFixture.CreateEmployeeInDepartmentAsync(hrClient, departmentId);

        var employeeUserId = Guid.NewGuid();
        var employeeRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient, employeeUserId, employeeRoleId, employeeId);

        using var employeeClient = CreateAuthenticatedClient("guest", employeeUserId);
        var create = await employeeClient.PostAsJsonAsync("/api/v1/timesheets", new
        {
            periodStart = "2026-03-01",
            periodEnd = "2026-03-07"
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var timesheet = await create.Content.ReadFromJsonAsync<TimesheetResponse>(JsonOptions);

        (await employeeClient.PostAsync($"/api/v1/timesheets/{timesheet!.Id}/submit", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var managerUserId = Guid.NewGuid();
        var managerRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.ManagerSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient,
            managerUserId,
            managerRoleId,
            attributes: new Dictionary<string, string> { ["departmentId"] = departmentId.ToString() });

        using var managerClient = CreateAuthenticatedClient("guest", managerUserId);
        var approve = await managerClient.PostAsync($"/api/v1/timesheets/{timesheet.Id}/approve", null);
        approve.StatusCode.Should().Be(HttpStatusCode.OK);
        var approved = await approve.Content.ReadFromJsonAsync<TimesheetResponse>(JsonOptions);
        approved!.Status.Should().Be("Approved");
    }

    private sealed record TimesheetResponse(Guid Id, string Status);
}

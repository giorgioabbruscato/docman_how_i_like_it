using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Domain;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class WorkflowLeaveApprovalTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public WorkflowLeaveApprovalTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task TwoStepLeaveWorkflow_CompletesAfterBothApprovals()
    {
        using var adminClient = CreateAuthenticatedClient("admin");
        var twoStepJson =
            """
            {"steps":[
              {"name":"Direct Manager","approverType":"DirectManager"},
              {"name":"HR","approverType":"Role","role":"leave.approve:team"}
            ]}
            """;

        var definitionResponse = await adminClient.PostAsJsonAsync("/api/v1/workflows/definitions", new
        {
            requestType = "Leave",
            name = "Two-step leave",
            stepsJson = twoStepJson
        });
        definitionResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var hrClient = CreateAuthenticatedClient("hr");
        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(hrClient);
        var employeeId = await TenantIsolationFixture.CreateEmployeeInDepartmentAsync(hrClient, departmentId);

        var employeeUserId = Guid.NewGuid();
        var employeeRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient, employeeUserId, employeeRoleId, employeeId);

        using var employeeClient = CreateAuthenticatedClient("guest", employeeUserId);
        var createResponse = await employeeClient.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId,
            startDate = "2026-01-10",
            endDate = "2026-01-12",
            type = "Annual"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var leave = await createResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>(JsonOptions);

        var managerUserId = Guid.NewGuid();
        var managerRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.ManagerSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient,
            managerUserId,
            managerRoleId,
            attributes: new Dictionary<string, string> { ["departmentId"] = departmentId.ToString() });

        using var managerClient = CreateAuthenticatedClient("guest", managerUserId);
        var managerApprove = await managerClient.PutAsync($"/api/v1/leave-requests/{leave!.Id}/approve", null);
        managerApprove.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterManager = await hrClient.GetFromJsonAsync<LeaveRequestResponse>(
            $"/api/v1/leave-requests/{leave.Id}",
            JsonOptions);
        afterManager!.Status.Should().Be("Pending");

        var hrUserId = Guid.NewGuid();
        var hrRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(hrClient, SystemRoleTemplates.HrSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient,
            hrUserId,
            hrRoleId,
            attributes: new Dictionary<string, string> { ["departmentId"] = departmentId.ToString() });

        using var hrApproverClient = CreateAuthenticatedClient("guest", hrUserId);
        var hrApprove = await hrApproverClient.PutAsync($"/api/v1/leave-requests/{leave.Id}/approve", null);
        hrApprove.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterHr = await hrClient.GetFromJsonAsync<LeaveRequestResponse>(
            $"/api/v1/leave-requests/{leave.Id}",
            JsonOptions);
        afterHr!.Status.Should().Be("Approved");
    }

    [Fact]
    public async Task Approve_ReturnsForbidden_ForUnauthorizedActor()
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
        var createResponse = await employeeClient.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId,
            startDate = "2026-02-01",
            endDate = "2026-02-03",
            type = "Sick"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var leave = await createResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>(JsonOptions);

        var outsiderUserId = Guid.NewGuid();
        var managerRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.ManagerSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient,
            outsiderUserId,
            managerRoleId,
            attributes: new Dictionary<string, string> { ["departmentId"] = Guid.NewGuid().ToString() });

        using var outsiderClient = CreateAuthenticatedClient("guest", outsiderUserId);
        var approveResponse = await outsiderClient.PutAsync($"/api/v1/leave-requests/{leave!.Id}/approve", null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Definitions_AreIsolatedByTenant()
    {
        using var platformClient = CreateClient("platform-admin", includeTenantHeader: false);
        var setup = await TenantIsolationFixture.CreateTwoTenantsAsync(platformClient);

        using var tenantAAdmin = CreateClientForTenant(setup.TenantASlug, "admin");
        var createA = await tenantAAdmin.PostAsJsonAsync("/api/v1/workflows/definitions", new
        {
            requestType = "Leave",
            name = "Tenant A leave",
            stepsJson = """{"steps":[{"name":"Manager","approverType":"DirectManager"}]}"""
        });
        createA.StatusCode.Should().Be(HttpStatusCode.Created);

        using var tenantBAdmin = CreateClientForTenant(setup.TenantBSlug, "admin");
        var listB = await tenantBAdmin.GetAsync("/api/v1/workflows/definitions");
        listB.StatusCode.Should().Be(HttpStatusCode.OK);
        var definitions = await listB.Content.ReadFromJsonAsync<List<WorkflowDefinitionResponse>>(JsonOptions);
        definitions!.Should().NotContain(d => d.Name == "Tenant A leave");
        definitions.Should().Contain(d => d.Name == "Leave Approval");
    }

    private sealed record LeaveRequestResponse(Guid Id, string Status);
    private sealed record WorkflowDefinitionResponse(Guid Id, string Name);
}

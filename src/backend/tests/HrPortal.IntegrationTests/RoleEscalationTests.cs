using System.Net;
using System.Net.Http.Json;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class RoleEscalationTests : IntegrationTestBase
{
    private static readonly Guid MissingId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public RoleEscalationTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Employee_CannotCreateEmployee()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.PostAsJsonAsync("/api/v1/employees", new
        {
            firstName = "Escalation",
            lastName = "Test",
            email = $"esc.{Guid.NewGuid():N}@demo.local",
            hireDate = "2024-01-15"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotCreateDepartment()
    {
        using var client = CreateAuthenticatedClient("employee");
        var code = $"E{Guid.NewGuid():N}"[..6].ToUpperInvariant();

        var response = await client.PostAsJsonAsync("/api/v1/departments", new
        {
            name = "Escalation Dept",
            code,
            description = "Should be forbidden"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotListDocuments()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync("/api/v1/documents");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotApproveLeaveRequest()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.PutAsync($"/api/v1/leave-requests/{MissingId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotAccessTeamAttendanceHistory()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var otherEmployeeId = await TenantIsolationFixture.CreateEmployeeAsync(hrClient, "escalation-att");

        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await client.GetAsync($"/api/v1/attendance/history?employeeId={otherEmployeeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Employee_CannotDeleteDocument()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.DeleteAsync($"/api/v1/documents/{MissingId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

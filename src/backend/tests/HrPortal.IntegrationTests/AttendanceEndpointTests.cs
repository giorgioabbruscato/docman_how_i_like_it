using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Application.Dtos;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class AttendanceEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AttendanceEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsForbidden_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync("/api/v1/attendance");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CheckIn_ReturnsNotFound_WhenEmployeeMissing()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.PostAsJsonAsync("/api/v1/attendance/check-in", new
        {
            employeeId = Guid.NewGuid()
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CheckIn_ReturnsCreated_ForOwnEmployee()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        var employeeId = await GetEmployeeIdAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/attendance/check-in", new
        {
            employeeId
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CheckIn_ReturnsForbidden_ForOtherEmployee()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var otherEmployeeId = await TenantIsolationFixture.CreateEmployeeAsync(hrClient);

        using var employeeClient = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await employeeClient.PostAsJsonAsync("/api/v1/attendance/check-in", new
        {
            employeeId = otherEmployeeId
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetReports_ReturnsOk_ForManagerRole()
    {
        using var client = CreateAuthenticatedClient("manager");

        var response = await client.GetAsync("/api/v1/attendance/reports?from=2025-01-01&to=2025-01-31");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<Guid> GetEmployeeIdAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.Content.ReadFromJsonAsync<MeDto>(JsonOptions);
        me!.EmployeeId.Should().NotBeNull();
        return me.EmployeeId!.Value;
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class AttendanceEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AttendanceEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Dashboard_ReturnsOk_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await client.GetAsync("/api/v1/attendance/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CheckIn_CreatesSession_ForOwnEmployee()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });

        var response = await client.PostAsJsonAsync("/api/v1/attendance/check-in", new
        {
            device = "Test Device"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });
    }

    [Fact]
    public async Task CheckIn_ReturnsForbidden_WithoutEmployeeContext()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.PostAsJsonAsync("/api/v1/attendance/check-in", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task History_ReturnsOk_ForEmployeeWithContext()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await client.GetAsync("/api/v1/attendance/history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

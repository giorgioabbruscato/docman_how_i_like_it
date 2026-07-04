using System.Net;
using System.Net.Http.Json;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class AttendanceEndpointTests : IntegrationTestBase
{
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
}

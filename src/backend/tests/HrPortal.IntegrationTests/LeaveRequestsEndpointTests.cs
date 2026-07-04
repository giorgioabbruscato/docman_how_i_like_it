using System.Net;
using System.Net.Http.Json;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class LeaveRequestsEndpointTests : IntegrationTestBase
{
    public LeaveRequestsEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsForbidden_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync("/api/v1/leave-requests");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenEmployeeMissing()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId = Guid.NewGuid(),
            startDate = "2025-07-01",
            endDate = "2025-07-05",
            type = "Annual"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class AttendanceCheckOutEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AttendanceCheckOutEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CheckOut_ClosesSession_AndReturnsWorkedMinutes()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });

        var checkInResponse = await client.PostAsJsonAsync("/api/v1/attendance/check-in", new
        {
            latitude = 45.4642,
            longitude = 9.19
        });
        checkInResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var checkOutResponse = await client.PostAsJsonAsync("/api/v1/attendance/check-out", new
        {
            latitude = 45.4642,
            longitude = 9.19,
            accuracy = 8.0
        });

        checkOutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await checkOutResponse.Content.ReadFromJsonAsync<CheckOutResponse>(JsonOptions);
        body!.WorkedMinutes.Should().BeGreaterThanOrEqualTo(0);
        body.Status.Should().Be("Closed");
        body.CheckOut.Should().BeAfter(body.CheckIn);
    }

    [Fact]
    public async Task CheckOut_ReturnsNotFound_WhenNoOpenSession()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });

        var response = await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record CheckOutResponse(
        Guid SessionId,
        DateTime CheckIn,
        DateTime CheckOut,
        int WorkedMinutes,
        string Status);
}

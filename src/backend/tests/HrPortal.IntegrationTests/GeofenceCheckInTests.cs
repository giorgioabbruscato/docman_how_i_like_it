using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class GeofenceCheckInTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GeofenceCheckInTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CheckIn_ReturnsBadRequest_WhenOutsideGeofence()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        await EnableGeofencingAsync(hrClient, allowWithoutGps: false);

        await hrClient.PostAsJsonAsync("/api/v1/geofence-zones", new
        {
            name = "Office",
            latitude = 45.0,
            longitude = 9.0,
            radiusMeters = 100
        });

        using var employeeClient = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        await EnsureNoOpenSessionAsync(employeeClient);

        var response = await employeeClient.PostAsJsonAsync("/api/v1/attendance/check-in", new
        {
            latitude = 46.0,
            longitude = 10.0,
            accuracy = 10.0
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("GEOFENCE_VIOLATION");
    }

    [Fact]
    public async Task CheckIn_ReturnsOk_WhenInsideGeofence()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        await EnableGeofencingAsync(hrClient, allowWithoutGps: false);

        await hrClient.PostAsJsonAsync("/api/v1/geofence-zones", new
        {
            name = "Office",
            latitude = 45.0,
            longitude = 9.0,
            radiusMeters = 500
        });

        using var employeeClient = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        await EnsureNoOpenSessionAsync(employeeClient);

        var response = await employeeClient.PostAsJsonAsync("/api/v1/attendance/check-in", new
        {
            latitude = 45.0001,
            longitude = 9.0001,
            accuracy = 10.0
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await employeeClient.PostAsJsonAsync("/api/v1/attendance/check-out", new { });
    }

    private static async Task EnableGeofencingAsync(HttpClient hrClient, bool allowWithoutGps)
    {
        var response = await hrClient.PutAsJsonAsync("/api/v1/geofence-zones/settings", new
        {
            geofencingEnabled = true,
            allowCheckInWithoutGps = allowWithoutGps
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task EnsureNoOpenSessionAsync(HttpClient client)
    {
        var statusResponse = await client.GetAsync("/api/v1/attendance/status");
        if (statusResponse.StatusCode != HttpStatusCode.OK)
            return;

        var status = await statusResponse.Content.ReadFromJsonAsync<AttendanceStatusResponse>(JsonOptions);
        if (status?.HasOpenSession == true)
        {
            await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });
        }
    }

    private sealed record AttendanceStatusResponse(bool HasOpenSession);
}

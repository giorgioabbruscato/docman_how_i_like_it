using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class AttendanceCheckInEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AttendanceCheckInEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CheckIn_CreatesOpenSession()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        await EnsureNoOpenSessionAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/attendance/check-in", new
        {
            latitude = 45.4642,
            longitude = 9.19,
            accuracy = 12.5,
            timezone = "Europe/Rome",
            device = "iPhone 15",
            browser = "Safari 17"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<AttendanceSessionResponse>(JsonOptions);
        session!.Status.Should().Be("Open");
        session.CheckOut.Should().BeNull();
        session.EmployeeId.Should().NotBeEmpty();

        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });
    }

    [Fact]
    public async Task CheckIn_ReturnsConflict_WhenOpenSessionExists()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        await EnsureNoOpenSessionAsync(client);

        var first = await client.PostAsJsonAsync("/api/v1/attendance/check-in", new { });
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await client.PostAsJsonAsync("/api/v1/attendance/check-in", new { });
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);

        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });
    }

    [Fact]
    public async Task CheckIn_ReturnsForbidden_WithoutPermission()
    {
        using var adminClient = CreateAuthenticatedClient("admin");
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(adminClient, "no-checkin-perm");
        var employeeUserId = Guid.NewGuid();

        var roleResponse = await adminClient.PostAsJsonAsync("/api/v1/roles", new
        {
            slug = $"view-only-{Guid.NewGuid():N}"[..20],
            permissions = new[] { "attendance_session.read:self" }
        });
        roleResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var role = await roleResponse.Content.ReadFromJsonAsync<RoleResponse>(JsonOptions);

        await TenantIsolationFixture.CreateMembershipAsync(
            adminClient,
            employeeUserId,
            role!.Id,
            employeeId);

        using var client = CreateAuthenticatedClient("guest", employeeUserId);

        var response = await client.PostAsJsonAsync("/api/v1/attendance/check-in", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static async Task EnsureNoOpenSessionAsync(HttpClient client)
    {
        await client.PostAsJsonAsync("/api/v1/attendance/check-out", new { });
    }

    private sealed record AttendanceSessionResponse(
        Guid Id,
        Guid EmployeeId,
        DateTime CheckIn,
        DateTime? CheckOut,
        string Status);

    private sealed record RoleResponse(Guid Id);
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class TimerEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TimerEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task StartGetActiveStopFlow_Works()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        await EnsureNoActiveTimerAsync(client);
        var projectId = await CreateProjectAsHrAsync();

        var startResponse = await client.PostAsJsonAsync("/api/v1/timer/start", new
        {
            projectId,
            description = "Timer test",
            billable = true
        });
        startResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var activeResponse = await client.GetAsync("/api/v1/timer/active");
        activeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var stopResponse = await client.PostAsync("/api/v1/timer/stop", null);
        stopResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var stopped = await stopResponse.Content.ReadFromJsonAsync<TimeEntryResponse>(JsonOptions);
        stopped!.WorkedMinutes.Should().BeGreaterThanOrEqualTo(0);
        stopped.EndTime.Should().NotBeNull();
    }

    [Fact]
    public async Task Start_ReturnsConflict_WhenActiveTimerExists()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        await EnsureNoActiveTimerAsync(client);
        var projectId = await CreateProjectAsHrAsync();

        var first = await client.PostAsJsonAsync("/api/v1/timer/start", new { projectId });
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/v1/timer/start", new { projectId });
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);

        await client.PostAsync("/api/v1/timer/stop", null);
    }

    [Fact]
    public async Task Stop_ReturnsNotFound_WhenNoActiveTimer()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await client.PostAsync("/api/v1/timer/stop", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Start_ReturnsConflict_WhenOverlappingClosedEntryExists()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(hrClient, "timer-overlap");
        var employeeUserId = Guid.NewGuid();
        var employeeRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(hrClient, employeeUserId, employeeRoleId, employeeId);

        using var client = CreateAuthenticatedClient("guest", employeeUserId);
        var projectId = await TenantIsolationFixture.CreateProjectAsync(hrClient, "overlap-proj");
        var now = DateTime.UtcNow;

        var createResponse = await client.PostAsJsonAsync("/api/v1/time-entries", new
        {
            projectId,
            startTime = now.AddMinutes(-30),
            endTime = now.AddMinutes(30)
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var startResponse = await client.PostAsJsonAsync("/api/v1/timer/start", new { projectId });
        startResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<Guid> CreateProjectAsHrAsync()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        return await TenantIsolationFixture.CreateProjectAsync(hrClient);
    }

    private static async Task EnsureNoActiveTimerAsync(HttpClient client)
    {
        await client.PostAsync("/api/v1/timer/stop", null);
    }

    private sealed record TimeEntryResponse(
        Guid Id,
        DateTime? EndTime,
        int WorkedMinutes);
}

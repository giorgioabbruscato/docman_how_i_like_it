using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class ManualTimeEntryEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ManualTimeEntryEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateManual_ReturnsCreated_ForValidRequest()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        var projectId = await CreateProjectAsHrAsync();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var response = await client.PostAsJsonAsync("/api/v1/time-entries/manual", new
        {
            date = date.ToString("yyyy-MM-dd"),
            projectId,
            hours = 2.5,
            description = "Manual work"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<TimeEntryResponse>(JsonOptions);
        body!.WorkedMinutes.Should().Be(150);
    }

    [Fact]
    public async Task CreateManual_ReturnsBadRequest_WhenDailyLimitExceeded()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        var projectId = await CreateProjectAsHrAsync();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));

        var first = await client.PostAsJsonAsync("/api/v1/time-entries/manual", new
        {
            date = date.ToString("yyyy-MM-dd"),
            projectId,
            hours = 20
        });
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/v1/time-entries/manual", new
        {
            date = date.ToString("yyyy-MM-dd"),
            projectId,
            hours = 5
        });
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateManual_ReturnsConflict_WhenOverlapping()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        var projectId = await CreateProjectAsHrAsync();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3));

        var first = await client.PostAsJsonAsync("/api/v1/time-entries/manual", new
        {
            date = date.ToString("yyyy-MM-dd"),
            projectId,
            hours = 2
        });
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/v1/time-entries/manual", new
        {
            date = date.ToString("yyyy-MM-dd"),
            projectId,
            hours = 1
        });
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<Guid> CreateProjectAsHrAsync()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        return await TenantIsolationFixture.CreateProjectAsync(hrClient);
    }

    private sealed record TimeEntryResponse(Guid Id, int WorkedMinutes);
}

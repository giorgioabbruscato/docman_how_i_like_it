using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HrPortal.Calendar.Application;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class TeamCalendarEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public TeamCalendarEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetEvents_ReturnsUnauthorized_WhenAnonymous()
    {
        using var client = CreateClient(includeTenantHeader: true);

        var response = await client.GetAsync(
            "/api/v1/calendar/events?fromDate=2026-01-01&toDate=2026-01-31");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEvents_ReturnsOk_ForHr()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.GetAsync(
            "/api/v1/calendar/events?fromDate=2026-01-01&toDate=2026-01-31");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var events = await response.Content.ReadFromJsonAsync<List<CalendarEventDto>>(JsonOptions);
        events.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEvents_IncludesCreatedHoliday()
    {
        using var client = CreateAuthenticatedClient("hr");
        var holidayDate = new DateOnly(2026, 12, 25);

        var createResponse = await client.PostAsJsonAsync("/api/v1/calendar/holidays", new
        {
            name = "Christmas",
            date = holidayDate.ToString("yyyy-MM-dd"),
            isRecurring = true
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var eventsResponse = await client.GetAsync(
            $"/api/v1/calendar/events?fromDate=2026-12-01&toDate=2026-12-31");
        eventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var events = await eventsResponse.Content.ReadFromJsonAsync<List<CalendarEventDto>>(JsonOptions);
        events!.Should().Contain(e =>
            e.Type == CalendarEventType.Holiday &&
            e.Title == "Christmas" &&
            e.StartDate == holidayDate);
    }

    [Fact]
    public async Task CreateHoliday_ReturnsForbidden_ForEmployee()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.PostAsJsonAsync("/api/v1/calendar/holidays", new
        {
            name = "Test Holiday",
            date = "2026-06-01"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

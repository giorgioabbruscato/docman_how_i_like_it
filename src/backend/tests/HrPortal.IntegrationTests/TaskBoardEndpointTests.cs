using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Domain;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Audit.Domain;
using HrPortal.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.IntegrationTests;

public sealed class TaskBoardEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TaskBoardEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetBoard_ReturnsAllFourColumns_WhenEmpty()
    {
        using var client = CreateAuthenticatedClient("hr");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "board-empty");

        var response = await client.GetAsync($"/api/v1/projects/{projectId}/tasks/board");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var board = await response.Content.ReadFromJsonAsync<TaskBoardResponse>(JsonOptions);
        board!.ProjectId.Should().Be(projectId);
        board.Columns.Should().HaveCount(4);
        board.Columns.Should().OnlyContain(c => c.Tasks.Count == 0);
        board.Columns.Select(c => c.Status).Should().Equal("Todo", "InProgress", "Review", "Done");
    }

    [Fact]
    public async Task GetBoard_GroupsTasksByStatus()
    {
        using var client = CreateAuthenticatedClient("hr");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "board-group");

        var todoResponse = await client.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId,
            title = "Todo task",
            priority = "Medium",
            status = "Todo"
        });
        todoResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var inProgressResponse = await client.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId,
            title = "Active task",
            priority = "High",
            status = "InProgress"
        });
        inProgressResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await client.GetAsync($"/api/v1/projects/{projectId}/tasks/board");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var board = await response.Content.ReadFromJsonAsync<TaskBoardResponse>(JsonOptions);
        board!.Columns.Single(c => c.Status == "Todo").Tasks.Should().HaveCount(1);
        board.Columns.Single(c => c.Status == "InProgress").Tasks.Should().HaveCount(1);
        board.Columns.Single(c => c.Status == "Review").Tasks.Should().BeEmpty();
        board.Columns.Single(c => c.Status == "Done").Tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBoard_ReturnsNotFound_WhenProjectMissing()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.GetAsync($"/api/v1/projects/{Guid.NewGuid()}/tasks/board");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBoard_ReturnsForbidden_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync($"/api/v1/projects/{Guid.NewGuid()}/tasks/board");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PatchStatus_UpdatesTask_ForHrRole()
    {
        using var client = CreateAuthenticatedClient("hr");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "patch-hr");
        var createResponse = await client.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId,
            title = "Move me",
            priority = "Medium",
            status = "Todo"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var task = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);

        var response = await PatchStatusAsync(client, task!.Id, "InProgress");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        updated!.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task PatchStatus_WritesAuditEntry()
    {
        using var client = CreateAuthenticatedClient("hr");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "patch-audit");
        var createResponse = await client.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId,
            title = "Audit status",
            priority = "Medium",
            status = "Todo"
        });
        var task = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);

        var response = await PatchStatusAsync(client, task!.Id, "Review");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var audit = await db.Set<AuditLog>()
            .IgnoreQueryFilters()
            .Where(a => a.EntityId == task.Id.ToString() && a.Action == "task.status_changed")
            .SingleOrDefaultAsync();

        audit.Should().NotBeNull();
        audit!.Metadata.Should().Contain("Todo");
        audit.Metadata.Should().Contain("Review");
    }

    [Fact]
    public async Task PatchStatus_ReturnsBadRequest_ForNoOpTransition()
    {
        using var client = CreateAuthenticatedClient("hr");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "patch-noop");
        var createResponse = await client.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId,
            title = "No-op",
            priority = "Medium",
            status = "Todo"
        });
        var task = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);

        var response = await PatchStatusAsync(client, task!.Id, "Todo");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PatchStatus_AllowsSelfScope_ForAssignee()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(hrClient, "self-scope");
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(hrClient, "assignee");
        var employeeUserId = Guid.NewGuid();
        var employeeRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(hrClient, employeeUserId, employeeRoleId, employeeId);

        var createResponse = await hrClient.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId,
            title = "My task",
            priority = "Medium",
            status = "Todo",
            assignedEmployeeId = employeeId
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var task = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);

        using var employeeClient = CreateAuthenticatedClient("guest", employeeUserId);
        var response = await PatchStatusAsync(employeeClient, task!.Id, "InProgress");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        updated!.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task PatchStatus_ReturnsForbidden_ForNonAssigneeEmployee()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(hrClient, "non-assignee");
        var assigneeId = await TenantIsolationFixture.CreateEmployeeAsync(hrClient, "assignee");
        var otherEmployeeId = await TenantIsolationFixture.CreateEmployeeAsync(hrClient, "other");
        var otherUserId = Guid.NewGuid();
        var employeeRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(hrClient, otherUserId, employeeRoleId, otherEmployeeId);

        var createResponse = await hrClient.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId,
            title = "Not yours",
            priority = "Medium",
            status = "Todo",
            assignedEmployeeId = assigneeId
        });
        var task = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);

        using var otherClient = CreateAuthenticatedClient("guest", otherUserId);
        var response = await PatchStatusAsync(otherClient, task!.Id, "InProgress");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static async Task<HttpResponseMessage> PatchStatusAsync(HttpClient client, Guid taskId, string status)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/tasks/{taskId}/status")
        {
            Content = System.Net.Http.Json.JsonContent.Create(new { status })
        };

        foreach (var header in client.DefaultRequestHeaders)
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return await client.SendAsync(request);
    }

    private sealed record TaskResponse(
        Guid Id,
        Guid ProjectId,
        string Title,
        Guid? AssignedEmployeeId,
        string Priority,
        string Status,
        decimal SpentHours);

    private sealed record TaskBoardResponse(
        Guid ProjectId,
        List<TaskBoardColumnResponse> Columns);

    private sealed record TaskBoardColumnResponse(
        string Status,
        List<TaskResponse> Tasks);
}

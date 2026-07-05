using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Application.Dtos;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class DocumentsEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DocumentsEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsForbidden_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync("/api/v1/documents");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync($"/api/v1/documents/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Upload_ReturnsCreated_ForOwnEmployee()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        var employeeId = await GetEmployeeIdAsync(client);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(employeeId.ToString()), "employeeId");
        content.Add(new StringContent("Contract"), "category");
        var fileContent = new ByteArrayContent("test document content"u8.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "contract.pdf");

        var response = await client.PostAsync("/api/v1/documents", content);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Upload_ReturnsForbidden_ForOtherEmployee()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var otherEmployeeId = await TenantIsolationFixture.CreateEmployeeAsync(hrClient);

        using var employeeClient = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(otherEmployeeId.ToString()), "employeeId");
        content.Add(new StringContent("Contract"), "category");
        var fileContent = new ByteArrayContent("test document content"u8.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "contract.pdf");

        var response = await employeeClient.PostAsync("/api/v1/documents", content);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static async Task<Guid> GetEmployeeIdAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.Content.ReadFromJsonAsync<MeDto>(JsonOptions);
        me!.EmployeeId.Should().NotBeNull();
        return me.EmployeeId!.Value;
    }
}

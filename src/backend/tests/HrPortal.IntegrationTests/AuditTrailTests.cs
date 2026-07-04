using System.Net;
using System.Net.Http.Json;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Audit.Domain;
using HrPortal.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.IntegrationTests;

public sealed class AuditTrailTests : IntegrationTestBase
{
    public AuditTrailTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateEmployee_WritesAuditLog()
    {
        var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        using var client = CreateAuthenticatedClient("hr", userId);
        var email = $"audit.{Guid.NewGuid():N}@demo.local";

        var response = await client.PostAsJsonAsync("/api/v1/employees", new
        {
            firstName = "Audit",
            lastName = "Trail",
            email,
            hireDate = "2024-01-15"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<EmployeeIdResponse>();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var auditLog = await dbContext.Set<AuditLog>()
            .AsNoTracking()
            .SingleOrDefaultAsync(a =>
                a.Entity == "Employee" &&
                a.EntityId == body!.Id.ToString() &&
                a.Action == "employee.created");

        auditLog.Should().NotBeNull();
        auditLog!.UserId.Should().Be(userId);
        auditLog.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        auditLog.TenantId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task SaveChanges_Throws_WhenAuditLogIsModified()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();

        var auditLog = AuditLog.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "test.action",
            "TestEntity",
            Guid.NewGuid().ToString());

        dbContext.Set<AuditLog>().Add(auditLog);
        await dbContext.SaveChangesAsync();

        auditLog = await dbContext.Set<AuditLog>().SingleAsync(a => a.Id == auditLog.Id);
        dbContext.Entry(auditLog).State = EntityState.Modified;

        var act = () => dbContext.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*immutable*");
    }

    [Fact]
    public async Task CreateTenant_WritesAuditLog()
    {
        var slug = $"audit{Guid.NewGuid():N}"[..12].ToLowerInvariant();
        using var client = CreateClient(includeTenantHeader: false);

        var response = await client.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "Audit Tenant",
            slug
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<TenantIdResponse>();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var auditLog = await dbContext.Set<AuditLog>()
            .AsNoTracking()
            .SingleOrDefaultAsync(a =>
                a.Entity == "Tenant" &&
                a.EntityId == body!.Id.ToString() &&
                a.Action == "tenant.created");

        auditLog.Should().NotBeNull();
        auditLog!.TenantId.Should().Be(body!.Id);
    }

    private sealed record EmployeeIdResponse(Guid Id);
    private sealed record TenantIdResponse(Guid Id);
}

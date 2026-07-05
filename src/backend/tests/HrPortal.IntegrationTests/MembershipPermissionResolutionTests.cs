using System.Net;
using System.Net.Http.Json;
using HrPortal.AccessControl.Application.Dtos;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

/// <summary>
/// Task 23 acceptance criteria: permission resolution must work identically for users with a real
/// <c>TenantMembership</c> (task 12 model) and for legacy Keycloak-role-only users falling back to
/// <c>LegacyRoleMapper</c> — see the ADR-012 addendum in architecture_decisions.md.
/// </summary>
public sealed class MembershipPermissionResolutionTests : IntegrationTestBase
{
    public MembershipPermissionResolutionTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task LegacyRoleOnlyUser_WithoutMembership_ResolvesPermissionsViaLegacyMapper()
    {
        using var client = CreateClient("employee", Guid.NewGuid());

        var response = await client.GetAsync("/api/v1/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var me = await response.Content.ReadFromJsonAsync<MeDto>();
        me!.RoleSlugs.Should().ContainSingle().Which.Should().Be(SystemRoleTemplates.EmployeeSlug);
        me.Permissions.Should().Contain(Permissions.EmployeeReadSelf);
        me.Permissions.Should().NotContain(Permissions.EmployeeReadTenant);
    }

    [Fact]
    public async Task MembershipUser_ResolvesPermissionsFromTenantRole_NotLegacyMapper()
    {
        using var adminClient = CreateClient("admin", DemoUsers.Admin);

        var rolesResponse = await adminClient.GetAsync("/api/v1/roles");
        rolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var roles = await rolesResponse.Content.ReadFromJsonAsync<List<TenantRoleDto>>();
        var hrRole = roles!.Single(r => r.Slug == SystemRoleTemplates.HrSlug);

        var membershipUserId = Guid.NewGuid();
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/memberships", new
        {
            userId = membershipUserId,
            roleIds = new[] { hrRole.Id },
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Authenticate as this user with an unmapped legacy role claim ("guest") so the only way
        // permissions can resolve to HR's permission set is via the real TenantMembership row.
        using var membershipClient = CreateClient("guest", membershipUserId);

        var meResponse = await membershipClient.GetAsync("/api/v1/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var me = await meResponse.Content.ReadFromJsonAsync<MeDto>();
        me!.RoleSlugs.Should().ContainSingle().Which.Should().Be(SystemRoleTemplates.HrSlug);
        me.Permissions.Should().Contain(Permissions.EmployeeReadTenant);
        me.Permissions.Should().Contain(Permissions.AuditReadTenant);

        // The permission actually gates a real endpoint: HR can list employees tenant-wide.
        var employeesResponse = await membershipClient.GetAsync("/api/v1/employees");
        employeesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

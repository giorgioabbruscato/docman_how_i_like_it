using FluentAssertions;
using HrPortal.AccessControl.Domain;
using HrPortal.Tenancy;

namespace HrPortal.UnitTests.Tenancy;

public sealed class TenantContextTests
{
    [Fact]
    public void HasPermission_ReturnsTrueWhenPermissionPresent()
    {
        var context = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with
        {
            Permissions = [Permissions.EmployeeReadSelf]
        };

        context.HasPermission(Permissions.EmployeeReadSelf).Should().BeTrue();
        context.HasPermission(Permissions.EmployeeReadTenant).Should().BeFalse();
    }

    [Fact]
    public void CreateTenantOnly_SetsResolvedWithMultiModeByDefault()
    {
        var tenantId = Guid.NewGuid();
        var context = TenantContext.CreateTenantOnly(tenantId, "demo");

        context.TenantId.Should().Be(tenantId);
        context.TenantSlug.Should().Be("demo");
        context.Mode.Should().Be(TenantDeploymentMode.Multi);
        context.IsResolved.Should().BeTrue();
        context.UserId.Should().BeNull();
    }

    [Fact]
    public void CreateSingleTenantDefault_SetsSingleMode()
    {
        var tenantId = Guid.NewGuid();
        var context = TenantContext.CreateSingleTenantDefault(tenantId, "demo");

        context.Mode.Should().Be(TenantDeploymentMode.Single);
        context.IsResolved.Should().BeTrue();
    }

    [Fact]
    public void Empty_IsNotResolved()
    {
        TenantContext.Empty.IsResolved.Should().BeFalse();
        TenantContext.Empty.TenantId.Should().Be(Guid.Empty);
    }
}

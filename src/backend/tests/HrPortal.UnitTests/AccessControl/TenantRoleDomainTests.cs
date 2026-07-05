using FluentAssertions;
using HrPortal.AccessControl.Domain;

namespace HrPortal.UnitTests.AccessControl;

public sealed class TenantRoleDomainTests
{
    [Fact]
    public void Create_NormalizesSlug()
    {
        var role = TenantRole.Create(Guid.NewGuid(), "Admin", ["employee.read:tenant"], isSystem: true);
        role.Slug.Should().Be("admin");
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var role = TenantRole.Create(Guid.NewGuid(), "custom", ["employee.read:tenant"], isSystem: false);
        role.Deactivate(Guid.NewGuid());
        role.IsActive.Should().BeFalse();
    }

    [Fact]
    public void GetPermissions_ReturnsDistinctPermissions()
    {
        var role = TenantRole.Create(
            Guid.NewGuid(),
            "custom",
            ["employee.read:self", "employee.read:self", "leave.create:self"],
            isSystem: false);

        role.GetPermissions().Should().HaveCount(2);
    }
}

using FluentAssertions;
using HrPortal.AccessControl.Domain;
using HrPortal.Identity;

namespace HrPortal.UnitTests.AccessControl;

public sealed class LegacyRoleMapperTests
{
    [Fact]
    public void Map_AdminRole_ReturnsAdminTemplatePermissions()
    {
        var permissions = LegacyRoleMapper.Map([Roles.Admin]);

        permissions.Should().Contain(Permissions.EmployeeReadTenant);
        permissions.Should().Contain(Permissions.RoleReadTenant);
    }

    [Fact]
    public void Map_EmployeeRole_ReturnsSelfScopedPermissions()
    {
        var permissions = LegacyRoleMapper.Map([Roles.Employee]);

        permissions.Should().Contain(Permissions.EmployeeReadSelf);
        permissions.Should().Contain(Permissions.LeaveCreateSelf);
        permissions.Should().NotContain(Permissions.EmployeeReadTenant);
    }
}

using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure;
using HrPortal.Tenancy;

namespace HrPortal.UnitTests.AccessControl;

public sealed class PolicyEngineTests
{
    private readonly IPolicyEngine _engine = new PolicyEngine(new ScopeResolver());

    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid EmployeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OtherEmployeeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid DepartmentId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public void Can_DeniesWhenPermissionMissing()
    {
        var ctx = CreateContext([Permissions.EmployeeReadTenant]);

        _engine.Can(ctx, Permissions.EmployeeDeleteTenant, null).Should().BeFalse();
    }

    [Fact]
    public void Can_AllowsPermissionOnlyWhenResourceIsNull()
    {
        var ctx = CreateContext([Permissions.EmployeeReadTenant]);

        _engine.Can(ctx, Permissions.EmployeeReadTenant, null).Should().BeTrue();
    }

    [Fact]
    public void Can_DeniesTenantScopeWhenResourceOutOfScope()
    {
        var ctx = CreateContext([Permissions.EmployeeReadTenant], employeeId: EmployeeId);
        var resource = new ResourceContext(EmployeeId: OtherEmployeeId, TenantId: Guid.NewGuid());

        _engine.Can(ctx, Permissions.EmployeeReadTenant, resource).Should().BeFalse();
    }

    [Fact]
    public void Can_AllowsTenantScopeWhenResourceInTenant()
    {
        var ctx = CreateContext([Permissions.EmployeeReadTenant], employeeId: EmployeeId);
        var resource = new ResourceContext(EmployeeId: OtherEmployeeId, TenantId: TenantId);

        _engine.Can(ctx, Permissions.EmployeeReadTenant, resource).Should().BeTrue();
    }

    [Fact]
    public void Can_DeniesSelfScopeWhenResourceOutOfScope()
    {
        var ctx = CreateContext([Permissions.EmployeeReadSelf], employeeId: EmployeeId);
        var resource = new ResourceContext(EmployeeId: OtherEmployeeId, TenantId: TenantId);

        _engine.Can(ctx, Permissions.EmployeeReadSelf, resource).Should().BeFalse();
    }

    [Fact]
    public void Can_AllowsSelfScopeWhenResourceMatchesEmployee()
    {
        var ctx = CreateContext([Permissions.EmployeeReadSelf], employeeId: EmployeeId);
        var resource = new ResourceContext(EmployeeId: EmployeeId, TenantId: TenantId);

        _engine.Can(ctx, Permissions.EmployeeReadSelf, resource).Should().BeTrue();
    }

    [Fact]
    public void Can_DeniesTeamScopeWhenResourceOutOfScope()
    {
        var ctx = CreateContext(
            [Permissions.LeaveApproveTeam],
            employeeId: EmployeeId,
            departmentId: DepartmentId);
        var resource = new ResourceContext(
            EmployeeId: OtherEmployeeId,
            DepartmentId: Guid.NewGuid(),
            TenantId: TenantId);

        _engine.Can(ctx, Permissions.LeaveApproveTeam, resource).Should().BeFalse();
    }

    [Fact]
    public void Can_AllowsTeamScopeWhenSameDepartment()
    {
        var ctx = CreateContext(
            [Permissions.LeaveApproveTeam],
            employeeId: EmployeeId,
            departmentId: DepartmentId);
        var resource = new ResourceContext(
            EmployeeId: OtherEmployeeId,
            DepartmentId: DepartmentId,
            TenantId: TenantId);

        _engine.Can(ctx, Permissions.LeaveApproveTeam, resource).Should().BeTrue();
    }

    [Fact]
    public void Can_AllowsAllScopeForPlatformAdmin()
    {
        var ctx = CreateContext([Permissions.TenantManageAll], isPlatformAdmin: true);
        var resource = new ResourceContext(TenantId: Guid.NewGuid());

        _engine.Can(ctx, Permissions.TenantManageAll, resource).Should().BeTrue();
    }

    [Fact]
    public void Can_DeniesAllScopeForNonPlatformAdmin()
    {
        var ctx = CreateContext([Permissions.TenantManageAll], isPlatformAdmin: false);
        var resource = new ResourceContext(TenantId: TenantId);

        _engine.Can(ctx, Permissions.TenantManageAll, resource).Should().BeFalse();
    }

    [Theory]
    [InlineData(Permissions.EmployeeReadTenant, AccessScope.Tenant)]
    [InlineData(Permissions.EmployeeReadSelf, AccessScope.Self)]
    [InlineData(Permissions.EmployeeReadTeam, AccessScope.Team)]
    [InlineData(Permissions.DepartmentReadTenant, AccessScope.Tenant)]
    [InlineData(Permissions.LeaveReadSelf, AccessScope.Self)]
    [InlineData(Permissions.AttendanceReadTeam, AccessScope.Team)]
    [InlineData(Permissions.DocumentReadSelf, AccessScope.Self)]
    [InlineData(Permissions.TenantManageAll, AccessScope.All)]
    public void TryParseScope_ParsesPermissionSuffix(string permission, AccessScope expected)
    {
        PolicyEngine.TryParseScope(permission, out var scope).Should().BeTrue();
        scope.Should().Be(expected);
    }

    [Fact]
    public void PermissionEvaluator_DelegatesToPolicyEngine()
    {
        var evaluator = new PermissionEvaluator(_engine);
        var ctx = CreateContext([Permissions.DepartmentReadTenant]);

        evaluator.Evaluate(ctx, Permissions.DepartmentReadTenant, new ResourceContext(TenantId: TenantId))
            .Should().BeTrue();
    }

    private static TenantContext CreateContext(
        IReadOnlyList<string> permissions,
        Guid? employeeId = null,
        Guid? departmentId = null,
        bool isPlatformAdmin = false) =>
        TenantContext.CreateTenantOnly(TenantId, "demo") with
        {
            Permissions = permissions,
            EmployeeId = employeeId,
            DepartmentId = departmentId,
            IsPlatformAdmin = isPlatformAdmin
        };
}

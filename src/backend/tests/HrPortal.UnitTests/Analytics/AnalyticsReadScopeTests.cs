using FluentAssertions;
using HrPortal.AccessControl.Domain;
using HrPortal.Analytics.Application;
using HrPortal.Employees.Application;
using HrPortal.Tenancy;
using Moq;

namespace HrPortal.UnitTests.Analytics;

public sealed class AnalyticsReadScopeTests
{
    private readonly Mock<IEmployeeLookup> _employeeLookup = new();
    private readonly Guid _departmentId = Guid.NewGuid();

    [Fact]
    public async Task ResolveAsync_ReturnsUnrestrictedFilter_ForTenantPermission()
    {
        var ctx = CreateContext([Permissions.AnalyticsReadTenant]);
        var employeeId = Guid.NewGuid();

        var result = await AnalyticsReadScope.ResolveAsync(ctx, _employeeLookup.Object, employeeId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AllowedEmployeeIds.Should().BeNull();
        result.Value.EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public async Task ResolveAsync_ReturnsDepartmentEmployees_ForTeamPermission()
    {
        var departmentEmployees = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _employeeLookup
            .Setup(l => l.GetActiveEmployeeIdsInDepartmentAsync(_departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(departmentEmployees);

        var ctx = CreateContext([Permissions.AnalyticsReadTeam], departmentId: _departmentId);

        var result = await AnalyticsReadScope.ResolveAsync(ctx, _employeeLookup.Object, null);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AllowedEmployeeIds.Should().BeEquivalentTo(departmentEmployees);
    }

    [Fact]
    public async Task ResolveAsync_ReturnsForbidden_WhenTeamPermissionWithoutDepartmentContext()
    {
        var ctx = CreateContext([Permissions.AnalyticsReadTeam]);

        var result = await AnalyticsReadScope.ResolveAsync(ctx, _employeeLookup.Object, null);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNotFound_WhenRequestedEmployeeOutsideTeam()
    {
        var allowed = new List<Guid> { Guid.NewGuid() };
        _employeeLookup
            .Setup(l => l.GetActiveEmployeeIdsInDepartmentAsync(_departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allowed);

        var ctx = CreateContext([Permissions.AnalyticsReadTeam], departmentId: _departmentId);

        var result = await AnalyticsReadScope.ResolveAsync(ctx, _employeeLookup.Object, Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task ResolveAsync_ReturnsForbidden_WhenMissingAnalyticsPermission()
    {
        var ctx = CreateContext([Permissions.EmployeeReadSelf]);

        var result = await AnalyticsReadScope.ResolveAsync(ctx, _employeeLookup.Object, null);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FORBIDDEN");
    }

    private static TenantContext CreateContext(
        IReadOnlyList<string> permissions,
        Guid? departmentId = null) =>
        TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with
        {
            Permissions = permissions,
            DepartmentId = departmentId
        };
}

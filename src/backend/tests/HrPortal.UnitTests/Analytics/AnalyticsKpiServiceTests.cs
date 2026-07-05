using FluentAssertions;
using HrPortal.Analytics.Application;
using HrPortal.Analytics.Application.Dtos;
using HrPortal.Analytics.Application.Options;
using HrPortal.Attendance.Application;
using HrPortal.Departments.Application;
using HrPortal.Employees.Application;
using HrPortal.Leave.Application;
using HrPortal.Projects.Application;
using HrPortal.Tasks.Application;
using HrPortal.Tenancy;
using HrPortal.TimeTracking.Application;
using Microsoft.Extensions.Options;
using Moq;

namespace HrPortal.UnitTests.Analytics;

public sealed class AnalyticsKpiServiceTests
{
    private readonly Mock<IEmployeeLookup> _employeeLookup = new();
    private readonly Mock<IDepartmentLookup> _departmentLookup = new();
    private readonly Mock<IProjectLookup> _projectLookup = new();
    private readonly Mock<ITimeEntryAnalyticsProvider> _timeEntryProvider = new();
    private readonly Mock<IAttendanceAnalyticsProvider> _attendanceProvider = new();
    private readonly Mock<ILeaveAnalyticsProvider> _leaveProvider = new();
    private readonly Mock<IProjectAnalyticsProvider> _projectProvider = new();
    private readonly Mock<ITaskAnalyticsProvider> _taskProvider = new();
    private readonly AnalyticsKpiService _service;

    public AnalyticsKpiServiceTests()
    {
        _service = new AnalyticsKpiService(
            TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo"),
            _employeeLookup.Object,
            _departmentLookup.Object,
            _projectLookup.Object,
            _timeEntryProvider.Object,
            _attendanceProvider.Object,
            _leaveProvider.Object,
            _projectProvider.Object,
            _taskProvider.Object,
            Options.Create(new AnalyticsOptions()));
    }

    [Fact]
    public async Task GetTotalWorkedHoursAsync_ConvertsMinutesToHours()
    {
        SetupTimeTotal(600);

        var result = await _service.GetTotalWorkedHoursAsync(CreateFilter());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10m);
    }

    [Fact]
    public async Task GetOvertimeHoursAsync_UsesConfiguredDailyStandard()
    {
        _timeEntryProvider
            .Setup(p => p.GetOvertimeMinutesAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<Guid>?>(),
                480,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(120);

        var result = await _service.GetOvertimeHoursAsync(CreateFilter());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2m);
    }

    [Fact]
    public async Task GetAttendanceRateAsync_ComputesPresentDaysOverExpectedWorkdays()
    {
        _attendanceProvider
            .Setup(p => p.GetPresentEmployeeDaysAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<Guid>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        _employeeLookup
            .Setup(e => e.CountActiveEmployeesAsync(It.IsAny<IReadOnlyList<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var filter = CreateFilter(new DateOnly(2025, 7, 7), new DateOnly(2025, 7, 11));
        var result = await _service.GetAttendanceRateAsync(filter);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1m);
    }

    [Fact]
    public async Task GetLeaveRateAsync_ReturnsZero_WhenDenominatorIsZero()
    {
        _leaveProvider
            .Setup(p => p.GetApprovedLeaveDaysAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<Guid>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _employeeLookup
            .Setup(e => e.CountActiveEmployeesAsync(It.IsAny<IReadOnlyList<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _service.GetLeaveRateAsync(CreateFilter());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0m);
    }

    [Fact]
    public async Task GetAverageHoursPerDayAsync_ReturnsZero_ForEmptyDailyTrend()
    {
        _timeEntryProvider
            .Setup(p => p.GetMinutesByDayAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<Guid>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _service.GetAverageHoursPerDayAsync(CreateFilter());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0m);
    }

    [Fact]
    public async Task GetHoursPerCustomerAsync_GroupsByCustomerName()
    {
        var projectA = Guid.NewGuid();
        var projectB = Guid.NewGuid();

        _timeEntryProvider
            .Setup(p => p.GetMinutesByProjectAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<Guid>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new MinutesByGuidRow(projectA, 120),
                new MinutesByGuidRow(projectB, 60)
            ]);

        _projectProvider
            .Setup(p => p.GetBudgetSnapshotsAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ProjectBudgetSnapshot(projectA, "P1", "Acme", 100m, 1000m),
                new ProjectBudgetSnapshot(projectB, "P2", "Acme", 50m, 500m)
            ]);

        var result = await _service.GetHoursPerCustomerAsync(CreateFilter());

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(r => r.Label == "Acme" && r.Hours == 3m);
    }

    private void SetupTimeTotal(int minutes)
    {
        _timeEntryProvider
            .Setup(p => p.GetTotalMinutesAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<Guid>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(minutes);
    }

    private static AnalyticsFilter CreateFilter(
        DateOnly? from = null,
        DateOnly? to = null) =>
        new(
            from ?? new DateOnly(2025, 7, 1),
            to ?? new DateOnly(2025, 7, 31),
            null,
            null,
            null,
            null);
}

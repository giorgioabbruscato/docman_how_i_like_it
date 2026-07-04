using HrPortal.Leave.Domain;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.UnitTests.Leave;

public sealed class LeaveRequestDomainTests
{
    [Fact]
    public void Create_SetsPendingStatus()
    {
        var request = LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2025, 6, 1),
            new DateOnly(2025, 6, 5),
            LeaveType.Annual);

        request.Status.Should().Be(LeaveStatus.Pending);
        request.DayCount.Should().Be(5);
    }

    [Fact]
    public void Create_Throws_WhenEndBeforeStart()
    {
        var act = () => LeaveRequest.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2025, 6, 10),
            new DateOnly(2025, 6, 5),
            LeaveType.Sick);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Approve_OnlyFromPending()
    {
        var request = LeaveRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 2), LeaveType.Sick);

        request.Approve(Guid.NewGuid());

        request.Status.Should().Be(LeaveStatus.Approved);
    }

    [Fact]
    public void Approve_Throws_WhenNotPending()
    {
        var request = LeaveRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 2), LeaveType.Sick);
        request.Approve(Guid.NewGuid());

        var act = () => request.Approve(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_OnlyFromPending()
    {
        var request = LeaveRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 2), LeaveType.Personal);

        request.Cancel(Guid.NewGuid());

        request.Status.Should().Be(LeaveStatus.Cancelled);
    }
}

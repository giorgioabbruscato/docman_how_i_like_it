using HrPortal.Attendance.Domain;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.UnitTests.Attendance;

public sealed class AttendanceSessionDomainTests
{
    [Fact]
    public void Close_SetsWorkedMinutes()
    {
        var checkIn = new DateTime(2026, 7, 5, 7, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 7, 5, 16, 0, 0, DateTimeKind.Utc);
        var session = AttendanceSession.Create(Guid.NewGuid(), Guid.NewGuid(), checkIn);

        session.Close(checkOut);

        session.WorkedMinutes.Should().Be(540);
        session.Status.Should().Be(AttendanceSessionStatus.Closed);
        session.CheckOut.Should().Be(checkOut);
    }

    [Fact]
    public void Close_Throws_WhenAlreadyClosed()
    {
        var checkIn = new DateTime(2026, 7, 5, 7, 0, 0, DateTimeKind.Utc);
        var session = AttendanceSession.Create(Guid.NewGuid(), Guid.NewGuid(), checkIn);
        session.Close(checkIn.AddHours(8));

        var act = () => session.Close(checkIn.AddHours(9));

        act.Should().Throw<DomainException>().WithMessage("*already closed*");
    }

    [Fact]
    public void Close_Throws_WhenCheckOutBeforeCheckIn()
    {
        var checkIn = new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc);
        var session = AttendanceSession.Create(Guid.NewGuid(), Guid.NewGuid(), checkIn);

        var act = () => session.Close(checkIn.AddHours(-1));

        act.Should().Throw<DomainException>().WithMessage("*after check-in*");
    }

    [Fact]
    public void CalculateWorkedMinutes_ReturnsRoundedMinutes()
    {
        var checkIn = new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc);
        var checkOut = checkIn.AddMinutes(90);

        AttendanceSession.CalculateWorkedMinutes(checkIn, checkOut).Should().Be(90);
    }
}

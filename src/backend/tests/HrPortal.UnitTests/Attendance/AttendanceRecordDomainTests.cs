using HrPortal.Attendance.Domain;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.UnitTests.Attendance;

public sealed class AttendanceRecordDomainTests
{
    [Fact]
    public void RecordCheckIn_SetsTime()
    {
        var record = AttendanceRecord.Create(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2025, 6, 1));

        record.RecordCheckIn(new TimeOnly(9, 0));

        record.CheckIn.Should().Be(new TimeOnly(9, 0));
    }

    [Fact]
    public void RecordCheckIn_Throws_WhenAlreadyCheckedIn()
    {
        var record = AttendanceRecord.Create(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2025, 6, 1));
        record.RecordCheckIn(new TimeOnly(9, 0));

        var act = () => record.RecordCheckIn(new TimeOnly(10, 0));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RecordCheckOut_Throws_WithoutCheckIn()
    {
        var record = AttendanceRecord.Create(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2025, 6, 1));

        var act = () => record.RecordCheckOut(new TimeOnly(17, 0));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RecordCheckOut_Throws_WhenBeforeCheckIn()
    {
        var record = AttendanceRecord.Create(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2025, 6, 1));
        record.RecordCheckIn(new TimeOnly(9, 0));

        var act = () => record.RecordCheckOut(new TimeOnly(8, 0));

        act.Should().Throw<DomainException>();
    }
}

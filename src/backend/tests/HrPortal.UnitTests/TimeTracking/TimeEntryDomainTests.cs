using HrPortal.TimeTracking.Domain;

namespace HrPortal.UnitTests.TimeTracking;

public sealed class TimeEntryDomainTests
{
    [Fact]
    public void CalculateWorkedMinutes_ReturnsExpectedValue()
    {
        var start = new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(2).AddMinutes(30);

        TimeEntry.CalculateWorkedMinutes(start, end).Should().Be(150);
    }

    [Fact]
    public void Stop_SetsEndTimeAndWorkedMinutes()
    {
        var start = new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc);
        var entry = TimeEntry.StartTimer(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start);
        var stopAt = start.AddMinutes(45);

        entry.Stop(stopAt);

        entry.EndTime.Should().Be(stopAt);
        entry.WorkedMinutes.Should().Be(45);
    }

    [Fact]
    public void Stop_ThrowsWhenAlreadyStopped()
    {
        var start = new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(1);
        var entry = TimeEntry.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end);

        var act = () => entry.Stop(end.AddMinutes(5));

        act.Should().Throw<SharedKernel.Exceptions.DomainException>()
            .WithMessage("*already stopped*");
    }
}

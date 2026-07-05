using HrPortal.TimeTracking.Domain;

namespace HrPortal.UnitTests.TimeTracking;

public sealed class TimesheetSubmissionDomainTests
{
    [Fact]
    public void Submit_Throws_WhenNotDraft()
    {
        var submission = TimesheetSubmission.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 7),
            480, []);

        submission.Submit();
        var act = () => submission.Submit();
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Approve_Succeeds_FromSubmitted()
    {
        var submission = TimesheetSubmission.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 7),
            480, []);

        submission.Submit();
        submission.Approve();
        submission.Status.Should().Be(TimesheetStatus.Approved);
    }
}

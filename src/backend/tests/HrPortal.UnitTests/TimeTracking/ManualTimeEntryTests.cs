using FluentValidation.TestHelper;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Application.Validators;

namespace HrPortal.UnitTests.TimeTracking;

public sealed class ManualTimeEntryTests
{
    private readonly CreateManualTimeEntryRequestValidator _validator = new();

    [Fact]
    public void Validator_RejectsFutureDate()
    {
        var request = new CreateManualTimeEntryRequest(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Guid.NewGuid(),
            2);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Validator_RejectsZeroHours()
    {
        var request = new CreateManualTimeEntryRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            Guid.NewGuid(),
            0);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Hours);
    }

    [Fact]
    public void Validator_RejectsMoreThan24Hours()
    {
        var request = new CreateManualTimeEntryRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            Guid.NewGuid(),
            25);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Hours);
    }

    [Fact]
    public void Validator_AcceptsValidRequest()
    {
        var request = new CreateManualTimeEntryRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            Guid.NewGuid(),
            2.5m);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

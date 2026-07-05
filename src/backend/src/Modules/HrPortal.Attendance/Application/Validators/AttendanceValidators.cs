using FluentValidation;
using HrPortal.Attendance.Application.Dtos;

namespace HrPortal.Attendance.Application.Validators;

public sealed class CheckInRequestValidator : AbstractValidator<CheckInRequest>
{
    public CheckInRequestValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.Accuracy)
            .GreaterThan(0)
            .When(x => x.Accuracy.HasValue);

        RuleFor(x => x.Timezone)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Timezone));

        RuleFor(x => x.Device)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Device));

        RuleFor(x => x.Browser)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Browser));
    }
}

public sealed class CheckOutRequestValidator : AbstractValidator<CheckOutRequest>
{
    public CheckOutRequestValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.Accuracy)
            .GreaterThan(0)
            .When(x => x.Accuracy.HasValue);

        RuleFor(x => x.Device)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Device));

        RuleFor(x => x.Browser)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Browser));
    }
}

public sealed class GetAttendanceHistoryQueryValidator : AbstractValidator<GetAttendanceHistoryQuery>
{
    public GetAttendanceHistoryQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}

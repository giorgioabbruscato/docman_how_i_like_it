using FluentValidation;
using HrPortal.TimeTracking.Application.Dtos;

namespace HrPortal.TimeTracking.Application.Validators;

public sealed class CreateTimesheetRequestValidator : AbstractValidator<CreateTimesheetRequest>
{
    public CreateTimesheetRequestValidator()
    {
        RuleFor(x => x.PeriodEnd)
            .GreaterThanOrEqualTo(x => x.PeriodStart)
            .WithMessage("Period end must be on or after period start.");
    }
}

public sealed class RejectTimesheetRequestValidator : AbstractValidator<RejectTimesheetRequest>
{
    public RejectTimesheetRequestValidator()
    {
        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .When(x => x.Comment is not null);
    }
}

public sealed class GetTimesheetsQueryValidator : AbstractValidator<GetTimesheetsQuery>
{
    public GetTimesheetsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

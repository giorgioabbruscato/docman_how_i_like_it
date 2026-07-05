using FluentValidation;
using HrPortal.TimeTracking.Application.Dtos;

namespace HrPortal.TimeTracking.Application.Validators;

public sealed class CreateTimeEntryRequestValidator : AbstractValidator<CreateTimeEntryRequest>
{
    public CreateTimeEntryRequestValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty().GreaterThan(x => x.StartTime);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}

public sealed class UpdateTimeEntryRequestValidator : AbstractValidator<UpdateTimeEntryRequest>
{
    public UpdateTimeEntryRequestValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty().GreaterThan(x => x.StartTime);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}

public sealed class GetTimeEntriesQueryValidator : AbstractValidator<GetTimeEntriesQuery>
{
    public GetTimeEntriesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class StartTimerRequestValidator : AbstractValidator<StartTimerRequest>
{
    public StartTimerRequestValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}

public sealed class CreateManualTimeEntryRequestValidator : AbstractValidator<CreateManualTimeEntryRequest>
{
    public CreateManualTimeEntryRequestValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Hours).GreaterThan(0).LessThanOrEqualTo(24);
        RuleFor(x => x.Date).LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date cannot be in the future.");
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}

public sealed class ExportTimeEntriesQueryValidator : AbstractValidator<ExportTimeEntriesQuery>
{
    private static readonly string[] AllowedFormats = ["csv", "xlsx", "pdf"];

    public ExportTimeEntriesQueryValidator()
    {
        RuleFor(x => x.Format).NotEmpty().Must(f => AllowedFormats.Contains(f, StringComparer.OrdinalIgnoreCase));
        RuleFor(x => x.Month).InclusiveBetween(1, 12).When(x => x.Month.HasValue);
        RuleFor(x => x.Year).GreaterThan(2000).When(x => x.Year.HasValue);
    }
}

using FluentValidation;

namespace HrPortal.Reporting.Application.Validators;

public sealed class ReportQueryParamsValidator : AbstractValidator<ReportQueryParams>
{
    private static readonly HashSet<string> AllowedFormats =
        new(StringComparer.OrdinalIgnoreCase) { "csv", "xlsx", "pdf" };

    public ReportQueryParamsValidator()
    {
        RuleFor(x => x.Format)
            .NotEmpty()
            .Must(f => AllowedFormats.Contains(f))
            .WithMessage("Format must be csv, xlsx, or pdf.");
    }
}

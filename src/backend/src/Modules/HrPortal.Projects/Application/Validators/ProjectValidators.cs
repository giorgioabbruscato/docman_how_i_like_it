using FluentValidation;
using HrPortal.Projects.Application.Dtos;

namespace HrPortal.Projects.Application.Validators;

public sealed class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
        RuleFor(x => x.BudgetHours).GreaterThanOrEqualTo(0).When(x => x.BudgetHours.HasValue);
        RuleFor(x => x.BudgetCost).GreaterThanOrEqualTo(0).When(x => x.BudgetCost.HasValue);
    }
}

public sealed class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
        RuleFor(x => x.BudgetHours).GreaterThanOrEqualTo(0).When(x => x.BudgetHours.HasValue);
        RuleFor(x => x.BudgetCost).GreaterThanOrEqualTo(0).When(x => x.BudgetCost.HasValue);
    }
}

public sealed class GetProjectsQueryValidator : AbstractValidator<GetProjectsQuery>
{
    public GetProjectsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class AddProjectMemberRequestValidator : AbstractValidator<AddProjectMemberRequest>
{
    public AddProjectMemberRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.HourlyRate).GreaterThanOrEqualTo(0).When(x => x.HourlyRate.HasValue);
    }
}

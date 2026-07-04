using FluentValidation;
using HrPortal.Leave.Application.Dtos;
using HrPortal.Leave.Domain;

namespace HrPortal.Leave.Application.Validators;

public sealed class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequest>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.StartDate).LessThanOrEqualTo(x => x.EndDate);
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => Enum.TryParse<LeaveType>(t, true, out _))
            .WithMessage("Invalid leave type.");
        RuleFor(x => x.Reason).MaximumLength(500).When(x => x.Reason is not null);
    }
}

public sealed class RejectLeaveRequestValidator : AbstractValidator<RejectLeaveRequest>
{
    public RejectLeaveRequestValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500).When(x => x.Reason is not null);
    }
}

using FluentValidation;
using HrPortal.Attendance.Application.Dtos;

namespace HrPortal.Attendance.Application.Validators;

public sealed class CheckInRequestValidator : AbstractValidator<CheckInRequest>
{
    public CheckInRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public sealed class CheckOutRequestValidator : AbstractValidator<CheckOutRequest>
{
    public CheckOutRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

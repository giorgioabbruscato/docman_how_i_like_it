using FluentValidation;
using HrPortal.Departments.Application.Dtos;

namespace HrPortal.Departments.Application.Validators;

public sealed class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Code may only contain letters, numbers, hyphens, and underscores.");
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
    }
}

public sealed class UpdateDepartmentRequestValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50)
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Code may only contain letters, numbers, hyphens, and underscores.");
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
    }
}

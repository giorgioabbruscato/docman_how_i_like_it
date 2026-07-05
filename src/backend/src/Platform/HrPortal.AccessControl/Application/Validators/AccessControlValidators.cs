using FluentValidation;
using HrPortal.AccessControl.Application.Dtos;

namespace HrPortal.AccessControl.Application.Validators;

public sealed class CreateTenantRoleRequestValidator : AbstractValidator<CreateTenantRoleRequest>
{
    public CreateTenantRoleRequestValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Permissions).NotNull();
        RuleForEach(x => x.Permissions).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateTenantRoleRequestValidator : AbstractValidator<UpdateTenantRoleRequest>
{
    public UpdateTenantRoleRequestValidator()
    {
        RuleFor(x => x.Permissions).NotNull();
        RuleForEach(x => x.Permissions).NotEmpty().MaximumLength(100);
    }
}

public sealed class CreateTenantMembershipRequestValidator : AbstractValidator<CreateTenantMembershipRequest>
{
    public CreateTenantMembershipRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleIds).NotNull().NotEmpty();
    }
}

public sealed class UpdateTenantMembershipRequestValidator : AbstractValidator<UpdateTenantMembershipRequest>
{
    public UpdateTenantMembershipRequestValidator()
    {
        RuleFor(x => x.RoleIds).NotNull().NotEmpty();
    }
}

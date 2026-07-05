using FluentValidation;
using HrPortal.Workflows.Application.Dtos;

namespace HrPortal.Workflows.Application.Validators;

public sealed class CreateWorkflowDefinitionRequestValidator : AbstractValidator<CreateWorkflowDefinitionRequest>
{
    public CreateWorkflowDefinitionRequestValidator()
    {
        RuleFor(x => x.RequestType).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StepsJson).NotEmpty();
    }
}

public sealed class UpdateWorkflowDefinitionRequestValidator : AbstractValidator<UpdateWorkflowDefinitionRequest>
{
    public UpdateWorkflowDefinitionRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StepsJson).NotEmpty();
    }
}

public sealed class ProcessWorkflowActionRequestValidator : AbstractValidator<ProcessWorkflowActionRequest>
{
    public ProcessWorkflowActionRequestValidator()
    {
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}

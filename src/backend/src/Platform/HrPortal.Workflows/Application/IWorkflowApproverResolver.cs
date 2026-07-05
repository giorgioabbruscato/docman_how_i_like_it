namespace HrPortal.Workflows.Application;

public sealed record WorkflowApprover(Guid UserId, Guid? EmployeeId);

public interface IWorkflowApproverResolver
{
    Task<IReadOnlyList<WorkflowApprover>> ResolveApproversAsync(
        Domain.WorkflowStepDefinition step,
        Guid requesterEmployeeId,
        CancellationToken cancellationToken = default);
}

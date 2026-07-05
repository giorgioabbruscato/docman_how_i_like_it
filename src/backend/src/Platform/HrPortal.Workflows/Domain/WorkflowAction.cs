using HrPortal.SharedKernel.Entities;

namespace HrPortal.Workflows.Domain;

public sealed class WorkflowAction : AuditableEntity
{
    public Guid WorkflowInstanceId { get; private set; }
    public int StepIndex { get; private set; }
    public Guid ActorEmployeeId { get; private set; }
    public WorkflowActionType Action { get; private set; }
    public string? Comment { get; private set; }
    public DateTime ActionAt { get; private set; }

    private WorkflowAction() { }

    public static WorkflowAction Record(
        Guid tenantId,
        Guid workflowInstanceId,
        int stepIndex,
        Guid actorEmployeeId,
        WorkflowActionType action,
        string? comment = null)
    {
        return new WorkflowAction
        {
            WorkflowInstanceId = workflowInstanceId,
            StepIndex = stepIndex,
            ActorEmployeeId = actorEmployeeId,
            Action = action,
            Comment = comment,
            ActionAt = DateTime.UtcNow
        }.Also(a => a.SetTenant(tenantId));
    }
}
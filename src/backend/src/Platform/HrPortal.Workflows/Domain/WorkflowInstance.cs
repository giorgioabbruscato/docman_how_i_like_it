using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.Workflows.Domain;

public sealed class WorkflowInstance : AuditableEntity
{
    public Guid WorkflowDefinitionId { get; private set; }
    public WorkflowRequestType RequestType { get; private set; }
    public Guid RequestId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public WorkflowStatus Status { get; private set; }
    public int CurrentStepIndex { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private WorkflowInstance() { }

    public static WorkflowInstance Create(
        Guid tenantId,
        Guid workflowDefinitionId,
        WorkflowRequestType requestType,
        Guid requestId,
        Guid employeeId,
        Guid? createdBy = null)
    {
        return new WorkflowInstance
        {
            WorkflowDefinitionId = workflowDefinitionId,
            RequestType = requestType,
            RequestId = requestId,
            EmployeeId = employeeId,
            Status = WorkflowStatus.InProgress,
            CurrentStepIndex = 0,
            StartedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        }.Also(i => i.SetTenant(tenantId));
    }

    public bool IsTerminal =>
        Status is WorkflowStatus.Approved or WorkflowStatus.Rejected or WorkflowStatus.Cancelled;

    public void AdvanceStep(Guid? updatedBy = null)
    {
        if (IsTerminal)
            throw new DomainException("Cannot advance a completed workflow.");

        CurrentStepIndex++;
        MarkUpdated(updatedBy);
    }

    public void MarkApproved(Guid? updatedBy = null)
    {
        if (IsTerminal)
            throw new DomainException("Workflow is already completed.");

        Status = WorkflowStatus.Approved;
        CompletedAt = DateTime.UtcNow;
        MarkUpdated(updatedBy);
    }

    public void MarkRejected(Guid? updatedBy = null)
    {
        if (IsTerminal)
            throw new DomainException("Workflow is already completed.");

        Status = WorkflowStatus.Rejected;
        CompletedAt = DateTime.UtcNow;
        MarkUpdated(updatedBy);
    }

    public void MarkCancelled(Guid? updatedBy = null)
    {
        if (IsTerminal)
            throw new DomainException("Workflow is already completed.");

        Status = WorkflowStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        MarkUpdated(updatedBy);
    }
}
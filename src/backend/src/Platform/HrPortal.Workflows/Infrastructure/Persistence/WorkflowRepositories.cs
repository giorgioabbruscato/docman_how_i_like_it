using HrPortal.Workflows.Application;
using HrPortal.Workflows.Domain;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Workflows.Infrastructure.Persistence;

internal sealed class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
{
    private readonly DbContext _dbContext;

    public WorkflowDefinitionRepository(DbContext dbContext) => _dbContext = dbContext;

    public Task<WorkflowDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Set<WorkflowDefinition>().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<WorkflowDefinition?> GetActiveByRequestTypeAsync(
        WorkflowRequestType requestType,
        CancellationToken cancellationToken = default) =>
        _dbContext.Set<WorkflowDefinition>()
            .FirstOrDefaultAsync(d => d.RequestType == requestType && d.IsActive, cancellationToken);

    public async Task<IReadOnlyList<WorkflowDefinition>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<WorkflowDefinition>()
            .OrderByDescending(d => d.Version)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<WorkflowDefinition>().AddAsync(definition, cancellationToken);

    public Task UpdateAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<WorkflowDefinition>().Update(definition);
        return Task.CompletedTask;
    }
}

internal sealed class WorkflowInstanceRepository : IWorkflowInstanceRepository
{
    private readonly DbContext _dbContext;

    public WorkflowInstanceRepository(DbContext dbContext) => _dbContext = dbContext;

    public Task<WorkflowInstance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Set<WorkflowInstance>().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<WorkflowInstance?> GetActiveByRequestAsync(
        WorkflowRequestType requestType,
        Guid requestId,
        CancellationToken cancellationToken = default) =>
        _dbContext.Set<WorkflowInstance>()
            .FirstOrDefaultAsync(
                i => i.RequestType == requestType
                     && i.RequestId == requestId
                     && i.Status != WorkflowStatus.Approved
                     && i.Status != WorkflowStatus.Rejected
                     && i.Status != WorkflowStatus.Cancelled,
                cancellationToken);

    public async Task<IReadOnlyList<WorkflowInstance>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<WorkflowInstance>()
            .OrderByDescending(i => i.StartedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WorkflowInstance>> GetActiveInProgressAsync(
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<WorkflowInstance>()
            .Where(i => i.Status == WorkflowStatus.InProgress)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(WorkflowInstance instance, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<WorkflowInstance>().AddAsync(instance, cancellationToken);

    public Task UpdateAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<WorkflowInstance>().Update(instance);
        return Task.CompletedTask;
    }
}

internal sealed class WorkflowActionRepository : IWorkflowActionRepository
{
    private readonly DbContext _dbContext;

    public WorkflowActionRepository(DbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<WorkflowAction>> GetByInstanceIdAsync(
        Guid instanceId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<WorkflowAction>()
            .Where(a => a.WorkflowInstanceId == instanceId)
            .OrderBy(a => a.ActionAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(WorkflowAction action, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<WorkflowAction>().AddAsync(action, cancellationToken);
}

using HrPortal.Tasks.Application;

namespace HrPortal.Tasks.Infrastructure;

internal sealed class TaskLookup : ITaskLookup
{
    private readonly IProjectTaskRepository _repository;

    public TaskLookup(IProjectTaskRepository repository) =>
        _repository = repository;

    public Task<bool> ExistsAsync(Guid taskId, CancellationToken cancellationToken = default) =>
        _repository.ExistsAsync(taskId, cancellationToken);
}

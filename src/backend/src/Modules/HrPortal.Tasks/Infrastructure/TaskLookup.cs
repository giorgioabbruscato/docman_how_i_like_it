using HrPortal.Tasks.Application;

namespace HrPortal.Tasks.Infrastructure;

internal sealed class TaskLookup : ITaskLookup
{
    private readonly IProjectTaskRepository _repository;

    public TaskLookup(IProjectTaskRepository repository) =>
        _repository = repository;

    public Task<bool> ExistsAsync(Guid taskId, CancellationToken cancellationToken = default) =>
        _repository.ExistsAsync(taskId, cancellationToken);

    public async Task<string?> GetTitleAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(taskId, cancellationToken);
        return task?.Title;
    }
}

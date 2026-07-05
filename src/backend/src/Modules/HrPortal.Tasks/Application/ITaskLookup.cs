namespace HrPortal.Tasks.Application;

/// <summary>
/// Public contract for cross-module task validation.
/// </summary>
public interface ITaskLookup
{
    Task<bool> ExistsAsync(Guid taskId, CancellationToken cancellationToken = default);
}

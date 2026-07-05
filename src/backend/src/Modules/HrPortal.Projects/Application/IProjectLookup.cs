namespace HrPortal.Projects.Application;

/// <summary>
/// Public contract for cross-module project validation.
/// </summary>
public interface IProjectLookup
{
    Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<string?> GetNameAsync(Guid projectId, CancellationToken cancellationToken = default);
}

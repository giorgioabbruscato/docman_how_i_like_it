namespace HrPortal.Projects.Infrastructure;

using HrPortal.Projects.Application;

internal sealed class ProjectLookup : IProjectLookup
{
    private readonly IProjectRepository _repository;

    public ProjectLookup(IProjectRepository repository) =>
        _repository = repository;

    public Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        _repository.ExistsAsync(projectId, cancellationToken);
}

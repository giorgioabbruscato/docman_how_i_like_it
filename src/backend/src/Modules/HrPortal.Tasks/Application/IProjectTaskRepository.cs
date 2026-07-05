using HrPortal.Tasks.Application.Dtos;
using HrPortal.Tasks.Domain;

namespace HrPortal.Tasks.Application;

public interface IProjectTaskRepository
{
    Task<ProjectTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ProjectTask>> GetPagedAsync(GetProjectTasksQuery query, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ProjectTask task, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProjectTask task, CancellationToken cancellationToken = default);
    Task DeleteAsync(ProjectTask task, CancellationToken cancellationToken = default);
}

using HrPortal.Projects.Application.Dtos;
using HrPortal.Projects.Domain;

namespace HrPortal.Projects.Application;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Project>> GetPagedAsync(GetProjectsQuery query, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
}

using HrPortal.Projects.Domain;

namespace HrPortal.Projects.Application;

public interface IProjectMemberRepository
{
    Task<ProjectMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectMember>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid projectId, Guid employeeId, CancellationToken cancellationToken = default);
    Task AddAsync(ProjectMember member, CancellationToken cancellationToken = default);
    Task RemoveAsync(ProjectMember member, CancellationToken cancellationToken = default);
}

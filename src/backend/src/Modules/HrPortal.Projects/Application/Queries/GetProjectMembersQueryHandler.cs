using HrPortal.Projects.Application.Dtos;
using HrPortal.SharedKernel.Results;

namespace HrPortal.Projects.Application.Queries;

public sealed class GetProjectMembersQueryHandler
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _memberRepository;

    public GetProjectMembersQueryHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository memberRepository)
    {
        _projectRepository = projectRepository;
        _memberRepository = memberRepository;
    }

    public async Task<Result<IReadOnlyList<ProjectMemberDto>>> HandleAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        if (!await _projectRepository.ExistsAsync(projectId, cancellationToken))
            return Result.Failure<IReadOnlyList<ProjectMemberDto>>("Project not found.", "NOT_FOUND");

        var members = await _memberRepository.GetByProjectIdAsync(projectId, cancellationToken);
        var dtos = members.Select(ProjectMapping.ToDto).ToList() as IReadOnlyList<ProjectMemberDto>;
        return Result.Success(dtos);
    }
}

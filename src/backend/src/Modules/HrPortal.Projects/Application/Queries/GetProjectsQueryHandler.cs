using HrPortal.Projects.Application.Dtos;
using HrPortal.SharedKernel.Results;

namespace HrPortal.Projects.Application.Queries;

public sealed class GetProjectsQueryHandler
{
    private readonly IProjectRepository _repository;

    public GetProjectsQueryHandler(IProjectRepository repository) =>
        _repository = repository;

    public async Task<Result<PagedResult<ProjectDto>>> HandleAsync(
        GetProjectsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = await _repository.GetPagedAsync(query, cancellationToken);
        var items = page.Items.Select(ProjectMapping.ToDto).ToList();
        return Result.Success(new PagedResult<ProjectDto>(items, page.TotalCount, page.Page, page.PageSize));
    }
}

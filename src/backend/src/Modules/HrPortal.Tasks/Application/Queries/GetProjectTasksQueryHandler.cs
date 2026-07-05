using HrPortal.Tasks.Application.Dtos;
using HrPortal.SharedKernel.Results;

namespace HrPortal.Tasks.Application.Queries;

public sealed class GetProjectTasksQueryHandler
{
    private readonly IProjectTaskRepository _repository;

    public GetProjectTasksQueryHandler(IProjectTaskRepository repository) =>
        _repository = repository;

    public async Task<Result<PagedResult<ProjectTaskDto>>> HandleAsync(
        GetProjectTasksQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = await _repository.GetPagedAsync(query, cancellationToken);
        var items = page.Items.Select(TaskMapping.ToDto).ToList();
        return Result.Success(new PagedResult<ProjectTaskDto>(items, page.TotalCount, page.Page, page.PageSize));
    }
}

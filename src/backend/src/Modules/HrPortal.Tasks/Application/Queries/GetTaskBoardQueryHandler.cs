using HrPortal.Projects.Application;
using HrPortal.Tasks.Application.Dtos;
using HrPortal.Tasks.Domain;
using HrPortal.SharedKernel.Results;
using DomainTaskStatus = HrPortal.Tasks.Domain.TaskStatus;

namespace HrPortal.Tasks.Application.Queries;

public sealed class GetTaskBoardQueryHandler
{
    private readonly IProjectTaskRepository _repository;
    private readonly IProjectLookup _projectLookup;

    public GetTaskBoardQueryHandler(IProjectTaskRepository repository, IProjectLookup projectLookup)
    {
        _repository = repository;
        _projectLookup = projectLookup;
    }

    public async Task<Result<TaskBoardDto>> HandleAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        if (!await _projectLookup.ExistsAsync(projectId, cancellationToken))
            return Result.Failure<TaskBoardDto>("Project not found.", "NOT_FOUND");

        var tasks = await _repository.GetByProjectIdAsync(projectId, cancellationToken);

        var columns = Enum.GetValues<DomainTaskStatus>()
            .Select(status => new TaskBoardColumnDto(
                status,
                tasks
                    .Where(t => t.Status == status)
                    .Select(TaskMapping.ToDto)
                    .ToList()))
            .ToList();

        return Result.Success(new TaskBoardDto(projectId, columns));
    }
}

using HrPortal.Tasks.Application.Dtos;
using HrPortal.SharedKernel.Results;

namespace HrPortal.Tasks.Application.Queries;

public sealed class GetProjectTaskByIdQueryHandler
{
    private readonly IProjectTaskRepository _repository;

    public GetProjectTaskByIdQueryHandler(IProjectTaskRepository repository) =>
        _repository = repository;

    public async Task<Result<ProjectTaskDto>> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result.Failure<ProjectTaskDto>("Task not found.", "NOT_FOUND");

        return Result.Success(TaskMapping.ToDto(task));
    }
}

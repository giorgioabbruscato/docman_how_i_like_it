using HrPortal.Projects.Application.Dtos;
using HrPortal.SharedKernel.Results;

namespace HrPortal.Projects.Application.Queries;

public sealed class GetProjectByIdQueryHandler
{
    private readonly IProjectRepository _repository;

    public GetProjectByIdQueryHandler(IProjectRepository repository) =>
        _repository = repository;

    public async Task<Result<ProjectDto>> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _repository.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return Result.Failure<ProjectDto>("Project not found.", "NOT_FOUND");

        return Result.Success(ProjectMapping.ToDto(project));
    }
}

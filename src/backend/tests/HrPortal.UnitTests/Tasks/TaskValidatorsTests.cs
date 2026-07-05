using HrPortal.Tasks.Application.Dtos;
using HrPortal.Tasks.Application.Validators;
using HrPortal.Tasks.Domain;
using DomainTaskStatus = HrPortal.Tasks.Domain.TaskStatus;

namespace HrPortal.UnitTests.Tasks;

public sealed class TaskValidatorsTests
{
    private readonly CreateProjectTaskRequestValidator _createValidator = new();
    private readonly UpdateProjectTaskRequestValidator _updateValidator = new();
    private readonly GetProjectTasksQueryValidator _queryValidator = new();

    [Fact]
    public void CreateProjectTaskRequestValidator_RejectsEmptyTitle()
    {
        var result = _createValidator.Validate(new CreateProjectTaskRequest(Guid.NewGuid(), ""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProjectTaskRequest.Title));
    }

    [Fact]
    public void CreateProjectTaskRequestValidator_RejectsEmptyProjectId()
    {
        var result = _createValidator.Validate(new CreateProjectTaskRequest(Guid.Empty, "Task"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProjectTaskRequest.ProjectId));
    }

    [Fact]
    public void CreateProjectTaskRequestValidator_RejectsNegativeEstimatedHours()
    {
        var result = _createValidator.Validate(new CreateProjectTaskRequest(
            Guid.NewGuid(), "Task", EstimatedHours: -1));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProjectTaskRequest.EstimatedHours));
    }

    [Fact]
    public void UpdateProjectTaskRequestValidator_RejectsNegativeSpentHours()
    {
        var result = _updateValidator.Validate(new UpdateProjectTaskRequest(
            Guid.NewGuid(), "Task", TaskPriority.Medium, DomainTaskStatus.Todo, SpentHours: -1));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProjectTaskRequest.SpentHours));
    }

    [Fact]
    public void GetProjectTasksQueryValidator_RejectsInvalidPage()
    {
        var result = _queryValidator.Validate(new GetProjectTasksQuery(Page: 0));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetProjectTasksQuery.Page));
    }

    [Fact]
    public void GetProjectTasksQueryValidator_RejectsPageSizeOver100()
    {
        var result = _queryValidator.Validate(new GetProjectTasksQuery(PageSize: 101));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetProjectTasksQuery.PageSize));
    }
}

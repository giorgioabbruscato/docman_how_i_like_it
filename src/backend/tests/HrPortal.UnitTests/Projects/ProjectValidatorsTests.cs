using HrPortal.Projects.Application.Dtos;
using HrPortal.Projects.Application.Validators;
using HrPortal.Projects.Domain;

namespace HrPortal.UnitTests.Projects;

public sealed class ProjectValidatorsTests
{
    private readonly CreateProjectRequestValidator _createValidator = new();
    private readonly UpdateProjectRequestValidator _updateValidator = new();
    private readonly GetProjectsQueryValidator _queryValidator = new();
    private readonly AddProjectMemberRequestValidator _memberValidator = new();

    [Fact]
    public void CreateProjectRequestValidator_RejectsEmptyName()
    {
        var result = _createValidator.Validate(new CreateProjectRequest(""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProjectRequest.Name));
    }

    [Fact]
    public void CreateProjectRequestValidator_RejectsInvalidDateRange()
    {
        var result = _createValidator.Validate(new CreateProjectRequest(
            "Project",
            ProjectStatus.Active,
            StartDate: new DateOnly(2025, 6, 1),
            EndDate: new DateOnly(2025, 5, 1)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProjectRequest.EndDate));
    }

    [Fact]
    public void CreateProjectRequestValidator_RejectsNegativeBudget()
    {
        var result = _createValidator.Validate(new CreateProjectRequest(
            "Project", ProjectStatus.Active, BudgetHours: -1));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProjectRequest.BudgetHours));
    }

    [Fact]
    public void UpdateProjectRequestValidator_RejectsEmptyName()
    {
        var result = _updateValidator.Validate(new UpdateProjectRequest("", ProjectStatus.Active));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProjectRequest.Name));
    }

    [Fact]
    public void GetProjectsQueryValidator_RejectsInvalidPage()
    {
        var result = _queryValidator.Validate(new GetProjectsQuery(Page: 0));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetProjectsQuery.Page));
    }

    [Fact]
    public void GetProjectsQueryValidator_RejectsPageSizeOver100()
    {
        var result = _queryValidator.Validate(new GetProjectsQuery(PageSize: 101));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetProjectsQuery.PageSize));
    }

    [Fact]
    public void AddProjectMemberRequestValidator_RejectsEmptyEmployeeId()
    {
        var result = _memberValidator.Validate(new AddProjectMemberRequest(
            Guid.Empty, ProjectMemberRole.Member));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddProjectMemberRequest.EmployeeId));
    }

    [Fact]
    public void AddProjectMemberRequestValidator_RejectsNegativeHourlyRate()
    {
        var result = _memberValidator.Validate(new AddProjectMemberRequest(
            Guid.NewGuid(), ProjectMemberRole.Member, -10m));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddProjectMemberRequest.HourlyRate));
    }
}

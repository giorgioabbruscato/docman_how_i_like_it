using HrPortal.Projects.Domain;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.UnitTests.Projects;

public sealed class ProjectTests
{
    [Fact]
    public void Create_SetsDefaults()
    {
        var tenantId = Guid.NewGuid();
        var project = Project.Create(tenantId, "Website Redesign", ProjectStatus.Active);

        project.TenantId.Should().Be(tenantId);
        project.Name.Should().Be("Website Redesign");
        project.Status.Should().Be(ProjectStatus.Active);
        project.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Create_Throws_WhenNameEmpty()
    {
        var act = () => Project.Create(Guid.NewGuid(), "  ", ProjectStatus.Active);
        act.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Fact]
    public void Create_Throws_WhenNameTooLong()
    {
        var act = () => Project.Create(Guid.NewGuid(), new string('x', 201), ProjectStatus.Active);
        act.Should().Throw<DomainException>().WithMessage("*200*");
    }

    [Fact]
    public void Create_Throws_WhenEndDateBeforeStartDate()
    {
        var act = () => Project.Create(
            Guid.NewGuid(),
            "Project",
            ProjectStatus.Active,
            startDate: new DateOnly(2025, 6, 1),
            endDate: new DateOnly(2025, 5, 1));

        act.Should().Throw<DomainException>().WithMessage("*End date*");
    }

    [Fact]
    public void Create_Throws_WhenBudgetHoursNegative()
    {
        var act = () => Project.Create(
            Guid.NewGuid(), "Project", ProjectStatus.Active, budgetHours: -1);

        act.Should().Throw<DomainException>().WithMessage("*Budget hours*");
    }

    [Fact]
    public void Create_Throws_WhenBudgetCostNegative()
    {
        var act = () => Project.Create(
            Guid.NewGuid(), "Project", ProjectStatus.Active, budgetCost: -0.01m);

        act.Should().Throw<DomainException>().WithMessage("*Budget cost*");
    }

    [Fact]
    public void Update_Throws_WhenEndDateBeforeStartDate()
    {
        var project = Project.Create(Guid.NewGuid(), "Project", ProjectStatus.Active);

        var act = () => project.Update(
            "Project",
            ProjectStatus.Active,
            null,
            null,
            new DateOnly(2025, 6, 1),
            new DateOnly(2025, 5, 1),
            null,
            null,
            Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archive_SetsIsArchivedTrue()
    {
        var project = Project.Create(Guid.NewGuid(), "Project", ProjectStatus.Active);

        project.Archive(Guid.NewGuid());

        project.IsArchived.Should().BeTrue();
    }
}

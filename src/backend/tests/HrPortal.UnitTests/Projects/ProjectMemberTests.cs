using HrPortal.Projects.Domain;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.UnitTests.Projects;

public sealed class ProjectMemberTests
{
    [Fact]
    public void Create_SetsFields()
    {
        var tenantId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var member = ProjectMember.Create(
            tenantId, projectId, employeeId, ProjectMemberRole.Lead, 75.50m);

        member.TenantId.Should().Be(tenantId);
        member.ProjectId.Should().Be(projectId);
        member.EmployeeId.Should().Be(employeeId);
        member.Role.Should().Be(ProjectMemberRole.Lead);
        member.HourlyRate.Should().Be(75.50m);
    }

    [Fact]
    public void Create_Throws_WhenHourlyRateNegative()
    {
        var act = () => ProjectMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ProjectMemberRole.Member,
            -1m);

        act.Should().Throw<DomainException>().WithMessage("*Hourly rate*");
    }
}

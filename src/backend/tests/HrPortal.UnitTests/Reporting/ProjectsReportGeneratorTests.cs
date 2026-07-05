using HrPortal.Projects.Application;
using HrPortal.Projects.Application.Dtos;
using HrPortal.Projects.Domain;
using HrPortal.Reporting.Application;
using HrPortal.Reporting.Application.Generators;
using HrPortal.TimeTracking.Application;
using Moq;

namespace HrPortal.UnitTests.Reporting;

public sealed class ProjectsReportGeneratorTests
{
    [Theory]
    [InlineData("csv")]
    [InlineData("xlsx")]
    [InlineData("pdf")]
    public async Task GenerateAsync_ProducesNonEmptyBytes_ForEachFormat(string format)
    {
        var project = Project.Create(Guid.NewGuid(), "Portal", ProjectStatus.Active, customerName: "Acme");

        var projectRepository = new Mock<IProjectRepository>();
        projectRepository.Setup(r => r.GetPagedAsync(It.IsAny<GetProjectsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Project>(new List<Project> { project }, 1, 1, 100));

        var memberRepository = new Mock<IProjectMemberRepository>();
        memberRepository.Setup(r => r.GetByProjectIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProjectMember>());

        var analytics = new Mock<ITimeEntryAnalyticsProvider>();
        analytics.Setup(p => p.GetMinutesByProjectAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<IReadOnlyList<Guid>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MinutesByGuidRow>());

        var generator = new ProjectsReportGenerator(
            projectRepository.Object,
            memberRepository.Object,
            analytics.Object);

        var scope = new ReportGenerateFilter(null, null);
        var query = new ReportQueryParams(format);

        var (content, _, _) = await generator.GenerateAsync(query, scope, CancellationToken.None);

        content.Should().NotBeEmpty();
    }
}

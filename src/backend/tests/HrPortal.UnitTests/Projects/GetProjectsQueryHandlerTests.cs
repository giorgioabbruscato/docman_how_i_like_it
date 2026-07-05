using HrPortal.Projects.Application;
using HrPortal.Projects.Application.Dtos;
using HrPortal.Projects.Application.Queries;
using HrPortal.Projects.Domain;
using Moq;

namespace HrPortal.UnitTests.Projects;

public sealed class GetProjectsQueryHandlerTests
{
    private readonly Mock<IProjectRepository> _repository = new();
    private readonly GetProjectsQueryHandler _handler;

    public GetProjectsQueryHandlerTests()
    {
        _handler = new GetProjectsQueryHandler(_repository.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPagedResults()
    {
        var project = Project.Create(Guid.NewGuid(), "Alpha", ProjectStatus.Active);
        var query = new GetProjectsQuery(Page: 1, PageSize: 20, Search: "alpha");

        _repository.Setup(r => r.GetPagedAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Project>([project], 1, 1, 20));

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items[0].Name.Should().Be("Alpha");
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyPage()
    {
        var query = new GetProjectsQuery(Search: "missing");

        _repository.Setup(r => r.GetPagedAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Project>([], 0, 1, 20));

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}

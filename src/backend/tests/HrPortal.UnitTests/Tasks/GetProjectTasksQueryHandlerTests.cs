using HrPortal.Tasks.Application;
using HrPortal.Tasks.Application.Dtos;
using HrPortal.Tasks.Application.Queries;
using HrPortal.Tasks.Domain;
using Moq;
using DomainTaskStatus = HrPortal.Tasks.Domain.TaskStatus;

namespace HrPortal.UnitTests.Tasks;

public sealed class GetProjectTasksQueryHandlerTests
{
    private readonly Mock<IProjectTaskRepository> _repository = new();
    private readonly GetProjectTasksQueryHandler _handler;

    public GetProjectTasksQueryHandlerTests()
    {
        _handler = new GetProjectTasksQueryHandler(_repository.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPagedResults()
    {
        var task = ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Alpha Task", TaskPriority.High);
        var query = new GetProjectTasksQuery(Page: 1, PageSize: 20, Search: "alpha");

        _repository.Setup(r => r.GetPagedAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<ProjectTask>([task], 1, 1, 20));

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items[0].Title.Should().Be("Alpha Task");
        result.Value.Items[0].Status.Should().Be(DomainTaskStatus.Todo);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyPage()
    {
        var query = new GetProjectTasksQuery(Search: "missing");

        _repository.Setup(r => r.GetPagedAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<ProjectTask>([], 0, 1, 20));

        var result = await _handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}

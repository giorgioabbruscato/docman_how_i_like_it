using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.Projects.Application;
using HrPortal.Tasks.Application;
using HrPortal.Tasks.Application.Commands;
using HrPortal.Tasks.Application.Dtos;
using HrPortal.Tasks.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HrPortal.UnitTests.Tasks;

public sealed class CreateProjectTaskCommandHandlerTests
{
    private readonly Mock<IProjectTaskRepository> _repository = new();
    private readonly Mock<IProjectLookup> _projectLookup = new();
    private readonly Mock<IEmployeeLookup> _employeeLookup = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly TenantContext _tenantContext = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with
    {
        UserId = Guid.NewGuid()
    };
    private readonly CreateProjectTaskCommandHandler _handler;

    public CreateProjectTaskCommandHandlerTests()
    {
        _projectLookup.Setup(l => l.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _handler = new CreateProjectTaskCommandHandler(
            _repository.Object,
            _projectLookup.Object,
            _employeeLookup.Object,
            _unitOfWork.Object,
            _tenantContext,
            _auditService.Object,
            NullLogger<CreateProjectTaskCommandHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_Succeeds_WhenValid()
    {
        var projectId = Guid.NewGuid();
        var result = await _handler.HandleAsync(new CreateProjectTaskRequest(projectId, "New Task"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("New Task");
        result.Value.ProjectId.Should().Be(projectId);
        _repository.Verify(r => r.AddAsync(It.IsAny<ProjectTask>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFound_WhenProjectMissing()
    {
        _projectLookup.Setup(l => l.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new CreateProjectTaskRequest(Guid.NewGuid(), "Task"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFound_WhenEmployeeInactive()
    {
        var employeeId = Guid.NewGuid();
        _employeeLookup.Setup(l => l.ExistsAndIsActiveAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(new CreateProjectTaskRequest(
            Guid.NewGuid(), "Task", AssignedEmployeeId: employeeId));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task HandleAsync_ReturnsValidationError_WhenDomainRulesFail()
    {
        var result = await _handler.HandleAsync(new CreateProjectTaskRequest(Guid.NewGuid(), "  "));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }
}

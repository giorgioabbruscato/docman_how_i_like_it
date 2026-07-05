using HrPortal.Audit.Application;
using HrPortal.Projects.Application;
using HrPortal.Projects.Application.Commands;
using HrPortal.Projects.Application.Dtos;
using HrPortal.Projects.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HrPortal.UnitTests.Projects;

public sealed class CreateProjectCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly TenantContext _tenantContext = TenantContext.CreateTenantOnly(Guid.NewGuid(), "demo") with
    {
        UserId = Guid.NewGuid()
    };
    private readonly CreateProjectCommandHandler _handler;

    public CreateProjectCommandHandlerTests()
    {
        _handler = new CreateProjectCommandHandler(
            _repository.Object,
            _unitOfWork.Object,
            _tenantContext,
            _auditService.Object,
            NullLogger<CreateProjectCommandHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_Succeeds_WhenValid()
    {
        var result = await _handler.HandleAsync(new CreateProjectRequest("New Project", ProjectStatus.Active));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("New Project");
        _repository.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ReturnsValidationError_WhenDomainRulesFail()
    {
        var result = await _handler.HandleAsync(new CreateProjectRequest("  ", ProjectStatus.Active));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }
}

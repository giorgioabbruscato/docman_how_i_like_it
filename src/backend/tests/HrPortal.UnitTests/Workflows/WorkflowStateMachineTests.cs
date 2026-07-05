using HrPortal.Audit.Application;
using HrPortal.Notifications;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using HrPortal.Workflows.Application;
using HrPortal.Workflows.Application.Dtos;
using HrPortal.Workflows.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HrPortal.UnitTests.Workflows;

public sealed class WorkflowStateMachineTests
{
    private readonly Mock<IWorkflowDefinitionRepository> _definitionRepository = new();
    private readonly Mock<IWorkflowInstanceRepository> _instanceRepository = new();
    private readonly Mock<IWorkflowActionRepository> _actionRepository = new();
    private readonly Mock<IWorkflowApproverResolver> _approverResolver = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly TenantContext _tenantContext;
    private readonly WorkflowEngine _engine;

    private static readonly string TwoStepJson =
        """
        {"steps":[
          {"name":"Manager","approverType":"DirectManager"},
          {"name":"HR","approverType":"Role","role":"leave.approve:team"}
        ]}
        """;

    private static readonly string OneStepJson =
        """{"steps":[{"name":"Manager","approverType":"DirectManager"}]}""";

    public WorkflowStateMachineTests()
    {
        var tenantId = Guid.NewGuid();
        _tenantContext = TenantContext.CreateTenantOnly(tenantId, "demo") with { UserId = Guid.NewGuid() };

        _engine = new WorkflowEngine(
            _definitionRepository.Object,
            _instanceRepository.Object,
            _actionRepository.Object,
            _approverResolver.Object,
            [],
            _unitOfWork.Object,
            _tenantContext,
            _auditService.Object,
            _notificationService.Object,
            NullLogger<WorkflowEngine>.Instance);
    }

    [Fact]
    public async Task Approve_AdvancesStep_OnMultiStepWorkflow()
    {
        var definition = WorkflowDefinition.Create(_tenantContext.TenantId, WorkflowRequestType.Leave, "Leave", TwoStepJson);
        var instance = WorkflowInstance.Create(_tenantContext.TenantId, definition.Id, WorkflowRequestType.Leave, Guid.NewGuid(), Guid.NewGuid());
        var managerUserId = Guid.NewGuid();
        var managerEmployeeId = Guid.NewGuid();

        SetupInstance(definition, instance);
        _approverResolver
            .Setup(r => r.ResolveApproversAsync(It.IsAny<WorkflowStepDefinition>(), instance.EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new WorkflowApprover(managerUserId, managerEmployeeId)]);

        var result = await _engine.ProcessActionAsync(
            instance.Id,
            WorkflowActionType.Approve,
            managerUserId,
            managerEmployeeId,
            null);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentStepIndex.Should().Be(1);
        result.Value.Status.Should().Be(nameof(WorkflowStatus.InProgress));
        instance.CurrentStepIndex.Should().Be(1);
    }

    [Fact]
    public async Task Reject_TerminatesWorkflow()
    {
        var definition = WorkflowDefinition.Create(_tenantContext.TenantId, WorkflowRequestType.Leave, "Leave", TwoStepJson);
        var instance = WorkflowInstance.Create(_tenantContext.TenantId, definition.Id, WorkflowRequestType.Leave, Guid.NewGuid(), Guid.NewGuid());
        var managerUserId = Guid.NewGuid();
        var managerEmployeeId = Guid.NewGuid();

        SetupInstance(definition, instance);
        _approverResolver
            .Setup(r => r.ResolveApproversAsync(It.IsAny<WorkflowStepDefinition>(), instance.EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new WorkflowApprover(managerUserId, managerEmployeeId)]);

        var result = await _engine.ProcessActionAsync(
            instance.Id,
            WorkflowActionType.Reject,
            managerUserId,
            managerEmployeeId,
            "Not approved");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(nameof(WorkflowStatus.Rejected));
        instance.Status.Should().Be(WorkflowStatus.Rejected);
        instance.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Approve_Completes_OnFinalStep()
    {
        var definition = WorkflowDefinition.Create(_tenantContext.TenantId, WorkflowRequestType.Leave, "Leave", OneStepJson);
        var instance = WorkflowInstance.Create(_tenantContext.TenantId, definition.Id, WorkflowRequestType.Leave, Guid.NewGuid(), Guid.NewGuid());
        var managerUserId = Guid.NewGuid();
        var managerEmployeeId = Guid.NewGuid();
        var completionCalled = false;

        var completionHandler = new Mock<IWorkflowCompletionHandler>();
        completionHandler.Setup(h => h.RequestType).Returns(WorkflowRequestType.Leave);
        completionHandler
            .Setup(h => h.HandleCompletionAsync(
                instance,
                WorkflowStatus.Approved,
                managerEmployeeId,
                null,
                It.IsAny<CancellationToken>()))
            .Callback(() => completionCalled = true)
            .ReturnsAsync(Result.Success());

        var engine = new WorkflowEngine(
            _definitionRepository.Object,
            _instanceRepository.Object,
            _actionRepository.Object,
            _approverResolver.Object,
            [completionHandler.Object],
            _unitOfWork.Object,
            _tenantContext,
            _auditService.Object,
            _notificationService.Object,
            NullLogger<WorkflowEngine>.Instance);

        SetupInstance(definition, instance);
        _approverResolver
            .Setup(r => r.ResolveApproversAsync(It.IsAny<WorkflowStepDefinition>(), instance.EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new WorkflowApprover(managerUserId, managerEmployeeId)]);

        var result = await engine.ProcessActionAsync(
            instance.Id,
            WorkflowActionType.Approve,
            managerUserId,
            managerEmployeeId,
            null);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(nameof(WorkflowStatus.Approved));
        completionCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAction_ReturnsForbidden_ForUnauthorizedActor()
    {
        var definition = WorkflowDefinition.Create(_tenantContext.TenantId, WorkflowRequestType.Leave, "Leave", OneStepJson);
        var instance = WorkflowInstance.Create(_tenantContext.TenantId, definition.Id, WorkflowRequestType.Leave, Guid.NewGuid(), Guid.NewGuid());

        SetupInstance(definition, instance);
        _approverResolver
            .Setup(r => r.ResolveApproversAsync(It.IsAny<WorkflowStepDefinition>(), instance.EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new WorkflowApprover(Guid.NewGuid(), Guid.NewGuid())]);

        var result = await _engine.ProcessActionAsync(
            instance.Id,
            WorkflowActionType.Approve,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("FORBIDDEN");
    }

    private void SetupInstance(WorkflowDefinition definition, WorkflowInstance instance)
    {
        _instanceRepository
            .Setup(r => r.GetByIdAsync(instance.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instance);
        _definitionRepository
            .Setup(r => r.GetByIdAsync(definition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);
        _actionRepository
            .Setup(r => r.GetByInstanceIdAsync(instance.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }
}

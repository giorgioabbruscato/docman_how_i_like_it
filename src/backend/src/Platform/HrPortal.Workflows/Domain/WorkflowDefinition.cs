using System.Text.Json;
using System.Text.Json.Serialization;
using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.Workflows.Domain;

public sealed class WorkflowDefinition : AuditableEntity
{
    public WorkflowRequestType RequestType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string StepsJson { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public int Version { get; private set; }

    private WorkflowDefinition() { }

    public static WorkflowDefinition Create(
        Guid tenantId,
        WorkflowRequestType requestType,
        string name,
        string stepsJson,
        int version = 1,
        Guid? createdBy = null)
    {
        ValidateStepsJson(stepsJson);

        return new WorkflowDefinition
        {
            RequestType = requestType,
            Name = name,
            StepsJson = stepsJson,
            IsActive = true,
            Version = version,
            CreatedBy = createdBy
        }.Also(d => d.SetTenant(tenantId));
    }

    public WorkflowDefinition CreateNewVersion(string name, string stepsJson, Guid? updatedBy = null)
    {
        ValidateStepsJson(stepsJson);

        return new WorkflowDefinition
        {
            RequestType = RequestType,
            Name = name,
            StepsJson = stepsJson,
            IsActive = true,
            Version = Version + 1,
            CreatedBy = updatedBy ?? CreatedBy
        }.Also(d => d.SetTenant(TenantId));
    }

    public void Deactivate(Guid? updatedBy = null)
    {
        IsActive = false;
        MarkUpdated(updatedBy);
    }

    public WorkflowStepsDefinition ParseSteps() =>
        JsonSerializer.Deserialize<WorkflowStepsDefinition>(StepsJson, WorkflowJsonOptions.Default)
        ?? throw new DomainException("Workflow steps JSON is invalid.");

    public static void ValidateStepsJson(string stepsJson)
    {
        if (string.IsNullOrWhiteSpace(stepsJson))
            throw new DomainException("Workflow steps are required.");

        var parsed = JsonSerializer.Deserialize<WorkflowStepsDefinition>(stepsJson, WorkflowJsonOptions.Default);
        if (parsed?.Steps is null || parsed.Steps.Count == 0)
            throw new DomainException("Workflow must contain at least one step.");
    }
}

public sealed class WorkflowStepsDefinition
{
    [JsonPropertyName("steps")]
    public List<WorkflowStepDefinition> Steps { get; set; } = [];
}

public sealed class WorkflowStepDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("approverType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ApproverType ApproverType { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("employeeId")]
    public Guid? EmployeeId { get; set; }
}

internal static class WorkflowJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
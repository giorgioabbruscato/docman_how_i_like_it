using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.Projects.Domain;

public sealed class Project : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? CustomerName { get; private set; }
    public ProjectStatus Status { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public decimal? BudgetHours { get; private set; }
    public decimal? BudgetCost { get; private set; }
    public bool IsArchived { get; private set; }

    private Project() { }

    public static Project Create(
        Guid tenantId,
        string name,
        ProjectStatus status,
        string? description = null,
        string? customerName = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        decimal? budgetHours = null,
        decimal? budgetCost = null,
        Guid? createdBy = null)
    {
        ValidateName(name);
        ValidateDateRange(startDate, endDate);
        ValidateBudgets(budgetHours, budgetCost);

        return new Project
        {
            Name = name,
            Description = description,
            CustomerName = customerName,
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            BudgetHours = budgetHours,
            BudgetCost = budgetCost,
            IsArchived = false,
            CreatedBy = createdBy
        }.Also(p => p.SetTenant(tenantId));
    }

    public void Update(
        string name,
        ProjectStatus status,
        string? description,
        string? customerName,
        DateOnly? startDate,
        DateOnly? endDate,
        decimal? budgetHours,
        decimal? budgetCost,
        Guid? updatedBy)
    {
        ValidateName(name);
        ValidateDateRange(startDate, endDate);
        ValidateBudgets(budgetHours, budgetCost);

        Name = name;
        Description = description;
        CustomerName = customerName;
        Status = status;
        StartDate = startDate;
        EndDate = endDate;
        BudgetHours = budgetHours;
        BudgetCost = budgetCost;
        MarkUpdated(updatedBy);
    }

    public void Archive(Guid? updatedBy)
    {
        IsArchived = true;
        MarkUpdated(updatedBy);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Project name is required.");

        if (name.Length > 200)
            throw new DomainException("Project name must not exceed 200 characters.");
    }

    private static void ValidateDateRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
            throw new DomainException("End date must be on or after start date.");
    }

    private static void ValidateBudgets(decimal? budgetHours, decimal? budgetCost)
    {
        if (budgetHours.HasValue && budgetHours.Value < 0)
            throw new DomainException("Budget hours must be greater than or equal to zero.");

        if (budgetCost.HasValue && budgetCost.Value < 0)
            throw new DomainException("Budget cost must be greater than or equal to zero.");
    }
}

internal static class ProjectExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HrPortal.Api.Infrastructure.OpenApi;

public sealed class ExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = "/" + (context.ApiDescription.RelativePath ?? string.Empty).TrimEnd('/');
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant();

        ApplyExamples(operation, path, method);
        ApplyErrorExamples(operation);
    }

    private static void ApplyExamples(OpenApiOperation operation, string path, string? method)
    {
        switch (path, method)
        {
            case ("/api/v1/tenants", "GET"):
                SetResponseExample(operation, "200", OpenApiExamples.TenantList);
                break;
            case ("/api/v1/tenants", "POST"):
                SetRequestExample(operation, OpenApiExamples.CreateTenantRequest);
                SetResponseExample(operation, "201", OpenApiExamples.TenantCreated);
                break;
            case ("/api/v1/employees", "GET"):
                SetResponseExample(operation, "200", OpenApiExamples.EmployeeList);
                break;
            case ("/api/v1/employees", "POST"):
                SetRequestExample(operation, OpenApiExamples.CreateEmployeeRequest);
                SetResponseExample(operation, "201", OpenApiExamples.Employee);
                break;
            case ("/api/v1/employees/{id}", "GET"):
            case ("/api/v1/employees/{id}", "PUT"):
                if (method == "PUT")
                    SetRequestExample(operation, OpenApiExamples.UpdateEmployeeRequest);
                SetResponseExample(operation, "200", OpenApiExamples.Employee);
                break;
            case ("/api/v1/departments", "GET"):
                SetResponseExample(operation, "200", OpenApiExamples.DepartmentList);
                break;
            case ("/api/v1/departments", "POST"):
                SetRequestExample(operation, OpenApiExamples.CreateDepartmentRequest);
                SetResponseExample(operation, "201", OpenApiExamples.Department);
                break;
            case ("/api/v1/departments/{id}", "GET"):
            case ("/api/v1/departments/{id}", "PUT"):
                if (method == "PUT")
                    SetRequestExample(operation, OpenApiExamples.CreateDepartmentRequest);
                SetResponseExample(operation, "200", OpenApiExamples.Department);
                break;
            case ("/api/v1/leave-requests", "GET"):
                SetResponseExample(operation, "200", OpenApiExamples.LeaveRequestList);
                break;
            case ("/api/v1/leave-requests", "POST"):
                SetRequestExample(operation, OpenApiExamples.CreateLeaveRequest);
                SetResponseExample(operation, "201", OpenApiExamples.LeaveRequest);
                break;
            case ("/api/v1/leave-requests/{id}", "GET"):
            case ("/api/v1/leave-requests/{id}/approve", "PUT"):
            case ("/api/v1/leave-requests/{id}/reject", "PUT"):
                if (path.EndsWith("/reject", StringComparison.Ordinal))
                    SetRequestExample(operation, OpenApiExamples.RejectLeaveRequest);
                SetResponseExample(operation, "200", OpenApiExamples.LeaveRequest);
                break;
            case ("/api/v1/attendance", "GET"):
                SetResponseExample(operation, "200", OpenApiExamples.AttendanceList);
                break;
            case ("/api/v1/attendance/check-in", "POST"):
            case ("/api/v1/attendance/check-out", "POST"):
                SetRequestExample(operation, OpenApiExamples.CheckInRequest);
                SetResponseExample(operation, "200", OpenApiExamples.AttendanceRecord);
                break;
            case ("/api/v1/attendance/reports", "GET"):
                SetResponseExample(operation, "200", OpenApiExamples.AttendanceReport);
                break;
            case ("/api/v1/documents", "GET"):
                SetResponseExample(operation, "200", OpenApiExamples.DocumentList);
                break;
            case ("/api/v1/documents/{id}", "GET"):
                SetResponseExample(operation, "200", OpenApiExamples.Document);
                break;
            case ("/api/v1/documents", "POST"):
                SetResponseExample(operation, "201", OpenApiExamples.Document);
                break;
        }
    }

    private static void ApplyErrorExamples(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("404", out var notFound))
            SetMediaExample(notFound, OpenApiExamples.ProblemDetailsNotFound);

        if (operation.Responses.TryGetValue("400", out var badRequest))
            SetMediaExample(badRequest, OpenApiExamples.ProblemDetailsBadRequest);
    }

    private static void SetRequestExample(OpenApiOperation operation, Microsoft.OpenApi.Any.IOpenApiAny example)
    {
        if (operation.RequestBody?.Content is null)
            return;

        if (operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
            mediaType.Example = example;
    }

    private static void SetResponseExample(OpenApiOperation operation, string statusCode, Microsoft.OpenApi.Any.IOpenApiAny example)
    {
        if (!operation.Responses.TryGetValue(statusCode, out var response))
            return;

        SetMediaExample(response, example);
    }

    private static void SetMediaExample(OpenApiResponse response, Microsoft.OpenApi.Any.IOpenApiAny example)
    {
        response.Content ??= new Dictionary<string, OpenApiMediaType>();
        if (!response.Content.ContainsKey("application/json"))
            response.Content["application/json"] = new OpenApiMediaType();

        response.Content["application/json"].Example = example;
    }
}

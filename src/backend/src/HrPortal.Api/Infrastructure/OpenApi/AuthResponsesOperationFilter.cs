using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HrPortal.Api.Infrastructure.OpenApi;

public sealed class AuthResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var allowAnonymous = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<IAllowAnonymous>()
            .Any();

        if (allowAnonymous)
            return;

        AddResponse(operation, "401", "Unauthorized — missing or invalid JWT.");
        AddResponse(operation, "403", "Forbidden — insufficient role for this policy.");
    }

    private static void AddResponse(OpenApiOperation operation, string statusCode, string description)
    {
        operation.Responses.TryAdd(statusCode, new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new() { Schema = OpenApiSchemas.ProblemDetails }
            }
        });
    }
}

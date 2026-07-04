using Microsoft.OpenApi.Models;

namespace HrPortal.Api.Infrastructure.OpenApi;

internal static class OpenApiSchemas
{
    public static readonly OpenApiSchema ProblemDetails = new()
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["type"] = new() { Type = "string", Format = "uri" },
            ["title"] = new() { Type = "string" },
            ["status"] = new() { Type = "integer", Format = "int32" },
            ["detail"] = new() { Type = "string" },
            ["errorCode"] = new() { Type = "string" }
        }
    };
}

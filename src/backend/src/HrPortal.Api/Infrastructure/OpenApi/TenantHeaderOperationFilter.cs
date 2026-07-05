using HrPortal.Tenancy;
using HrPortal.Tenancy.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HrPortal.Api.Infrastructure.OpenApi;

public sealed class TenantHeaderOperationFilter : IOperationFilter
{
    private readonly TenantResolverOptions _options;

    public TenantHeaderOperationFilter(IOptions<TenantResolverOptions> options) =>
        _options = options.Value;

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (IsExcludedPath(context.ApiDescription.RelativePath))
            return;

        var isSingleMode = _options.Mode == TenantDeploymentMode.Single;

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = _options.TenantHeaderName,
            In = ParameterLocation.Header,
            Required = !isSingleMode,
            Description = isSingleMode
                ? "Tenant slug (e.g. demo). Optional in single-tenant mode — defaults to configured tenant when omitted."
                : "Tenant slug (e.g. demo). Required on all business endpoints.",
            Schema = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("demo") }
        });
    }

    private static bool IsExcludedPath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        var path = "/" + relativePath.TrimEnd('/');
        return path.StartsWith("/api/v1/tenants", StringComparison.OrdinalIgnoreCase);
    }
}

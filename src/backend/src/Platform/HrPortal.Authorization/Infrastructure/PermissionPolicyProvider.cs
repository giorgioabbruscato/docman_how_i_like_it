using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HrPortal.Authorization.Infrastructure;

internal sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) =>
        _fallback = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(Policies.PermissionAnyPrefix, StringComparison.Ordinal))
        {
            var permissions = policyName[Policies.PermissionAnyPrefix.Length..]
                .Split(Policies.PermissionAnySeparator, StringSplitOptions.RemoveEmptyEntries);

            var anyPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionAnyRequirement(permissions))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(anyPolicy);
        }

        if (policyName.StartsWith(Policies.PermissionPrefix, StringComparison.Ordinal))
        {
            var permission = policyName[Policies.PermissionPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }
}

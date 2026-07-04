using System.Security.Claims;
using System.Text.Json;
using HrPortal.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HrPortal.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddHrPortalIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var environment = configuration["ASPNETCORE_ENVIRONMENT"]
            ?? configuration["DOTNET_ENVIRONMENT"]
            ?? "Production";

        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));

        if (!string.Equals(environment, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            var keycloakOptions = configuration
                .GetSection(KeycloakOptions.SectionName)
                .Get<KeycloakOptions>() ?? new KeycloakOptions();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = keycloakOptions.Authority;
                    options.Audience = keycloakOptions.Audience;
                    options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        RoleClaimType = ClaimTypes.Role
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var identity = context.Principal?.Identity as ClaimsIdentity;
                            if (identity is null)
                            {
                                return Task.CompletedTask;
                            }

                            var realmAccessClaim = context.Principal?.FindFirst("realm_access")?.Value;
                            if (string.IsNullOrWhiteSpace(realmAccessClaim))
                            {
                                return Task.CompletedTask;
                            }

                            try
                            {
                                using var document = JsonDocument.Parse(realmAccessClaim);
                                if (!document.RootElement.TryGetProperty("roles", out var rolesElement))
                                {
                                    return Task.CompletedTask;
                                }

                                foreach (var role in rolesElement.EnumerateArray())
                                {
                                    var roleName = role.GetString();
                                    if (!string.IsNullOrWhiteSpace(roleName))
                                    {
                                        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                    }
                                }
                            }
                            catch (JsonException)
                            {
                                // Ignore malformed realm_access claim
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
        }

        services.AddHttpContextAccessor();
        services.AddScoped(sp =>
        {
            var accessor = sp.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            return UserContextFactory.FromHttpContext(accessor.HttpContext);
        });

        return services;
    }
}

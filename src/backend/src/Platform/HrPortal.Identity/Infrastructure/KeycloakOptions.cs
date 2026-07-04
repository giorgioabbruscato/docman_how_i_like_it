namespace HrPortal.Identity.Infrastructure;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string[] ValidIssuers { get; set; } = [];
    public bool RequireHttpsMetadata { get; set; } = true;
}

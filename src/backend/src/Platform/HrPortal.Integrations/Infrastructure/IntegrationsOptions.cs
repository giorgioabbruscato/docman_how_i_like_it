using HrPortal.Integrations.Domain;

namespace HrPortal.Integrations.Infrastructure;

public sealed class IntegrationsOptions
{
    public const string SectionName = "Integrations";

    public bool UseMockProviders { get; set; }

    public GoogleCalendarOptions Google { get; set; } = new();

    public MicrosoftCalendarOptions Microsoft { get; set; } = new();

    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";

    public string ApiBaseUrl { get; set; } = "http://localhost:5000";
}

public sealed class GoogleCalendarOptions
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;
}

public sealed class MicrosoftCalendarOptions
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;
}

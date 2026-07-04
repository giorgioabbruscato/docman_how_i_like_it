namespace HrPortal.Identity;

public sealed class UserContext
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = [];
    public bool IsAuthenticated { get; init; }

    public static UserContext Anonymous => new() { IsAuthenticated = false };

    public bool IsInRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}

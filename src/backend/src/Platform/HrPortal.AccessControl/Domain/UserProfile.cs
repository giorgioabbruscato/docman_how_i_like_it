using HrPortal.SharedKernel.Entities;

namespace HrPortal.AccessControl.Domain;

public sealed class UserProfile : Entity
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public bool IsPlatformAdmin { get; private set; }

    private UserProfile() { }

    public static UserProfile Create(Guid userId, string email, bool isPlatformAdmin = false) =>
        new()
        {
            UserId = userId,
            Email = email.ToLowerInvariant(),
            IsPlatformAdmin = isPlatformAdmin
        };

    public void UpdateEmail(string email) =>
        Email = email.ToLowerInvariant();
}

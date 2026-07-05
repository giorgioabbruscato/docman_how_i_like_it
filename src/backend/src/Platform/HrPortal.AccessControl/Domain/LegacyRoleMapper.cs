using HrPortal.Identity;

namespace HrPortal.AccessControl.Domain;

public static class LegacyRoleMapper
{
    public static IReadOnlyList<string> Map(IReadOnlyList<string> keycloakRoles)
    {
        var permissions = new HashSet<string>(StringComparer.Ordinal);

        foreach (var role in keycloakRoles)
        {
            var slug = MapRoleToSlug(role);
            if (slug is null)
                continue;

            foreach (var permission in SystemRoleTemplates.GetPermissions(slug))
                permissions.Add(permission);
        }

        return permissions.OrderBy(p => p, StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<string> MapRoleSlugs(IReadOnlyList<string> keycloakRoles)
    {
        var slugs = new List<string>();

        foreach (var role in keycloakRoles)
        {
            var slug = MapRoleToSlug(role);
            if (slug is not null && !slugs.Contains(slug, StringComparer.Ordinal))
                slugs.Add(slug);
        }

        return slugs;
    }

    private static string? MapRoleToSlug(string role) =>
        role.Trim() switch
        {
            Roles.Admin => SystemRoleTemplates.AdminSlug,
            Roles.Hr => SystemRoleTemplates.HrSlug,
            Roles.Manager => SystemRoleTemplates.ManagerSlug,
            Roles.Employee => SystemRoleTemplates.EmployeeSlug,
            _ => null
        };
}

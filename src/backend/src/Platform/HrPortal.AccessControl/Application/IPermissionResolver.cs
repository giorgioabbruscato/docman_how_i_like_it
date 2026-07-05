namespace HrPortal.AccessControl.Application;

public interface IPermissionResolver
{
    Task<IReadOnlyList<string>> ResolveAsync(
        IReadOnlyList<Guid> roleIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ResolveRoleSlugsAsync(
        IReadOnlyList<Guid> roleIds,
        CancellationToken cancellationToken = default);
}

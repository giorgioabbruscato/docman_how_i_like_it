namespace HrPortal.Api.Infrastructure.OpenApi;

public sealed record TenantListItemDto(Guid Id, string Name, string Slug, bool IsActive);

public sealed record TenantCreatedDto(Guid Id, string Name, string Slug);

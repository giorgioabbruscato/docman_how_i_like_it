namespace HrPortal.SharedKernel.Exceptions;

public class TenantNotResolvedException : DomainException
{
    public TenantNotResolvedException()
        : base("Tenant context is not resolved.", "TENANT_NOT_RESOLVED")
    {
    }
}

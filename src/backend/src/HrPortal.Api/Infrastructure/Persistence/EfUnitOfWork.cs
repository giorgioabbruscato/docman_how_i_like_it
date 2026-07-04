using HrPortal.SharedKernel.Persistence;

namespace HrPortal.Api.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly HrPortalDbContext _dbContext;

    public EfUnitOfWork(HrPortalDbContext dbContext) => _dbContext = dbContext;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}

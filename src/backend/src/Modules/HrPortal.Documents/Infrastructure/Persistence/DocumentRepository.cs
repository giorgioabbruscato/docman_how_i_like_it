using HrPortal.Documents.Application;
using HrPortal.Documents.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Documents.Infrastructure.Persistence;

internal sealed class DocumentRepository : IDocumentRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public DocumentRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Document>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Document>()
            .ApplyTenantScope(_accessor.Current)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<Document>().AddAsync(document, cancellationToken);

    public Task DeleteAsync(Document document, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<Document>().Remove(document);
        return Task.CompletedTask;
    }
}

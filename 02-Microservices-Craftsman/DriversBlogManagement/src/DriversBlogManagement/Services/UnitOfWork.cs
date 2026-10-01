namespace DriversBlogManagement.Services;

using DriversBlogManagement.Databases;

public interface IUnitOfWork : IDriversBlogManagementScopedService
{
    Task<int> CommitChanges(CancellationToken cancellationToken = default);
}

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BlogManagementContext _dbContext;

    public UnitOfWork(BlogManagementContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CommitChanges(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

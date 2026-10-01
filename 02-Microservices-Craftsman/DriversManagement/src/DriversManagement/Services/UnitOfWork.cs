namespace DriversManagement.Services;

using DriversManagement.Databases;

public interface IUnitOfWork : IDriversManagementScopedService
{
    Task<int> CommitChanges(CancellationToken cancellationToken = default);
}

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DriversManagementContext _dbContext;

    public UnitOfWork(DriversManagementContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CommitChanges(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

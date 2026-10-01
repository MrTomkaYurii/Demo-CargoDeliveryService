namespace CargoOrderingService.Services;

using CargoOrderingService.Databases;

public interface IUnitOfWork : ICargoOrderingServiceScopedService
{
    Task<int> CommitChanges(CancellationToken cancellationToken = default);
}

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OrderingContext _dbContext;

    public UnitOfWork(OrderingContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CommitChanges(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

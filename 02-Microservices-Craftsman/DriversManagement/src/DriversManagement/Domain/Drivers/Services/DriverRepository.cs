namespace DriversManagement.Domain.Drivers.Services;

using DriversManagement.Domain.Drivers;
using DriversManagement.Databases;
using DriversManagement.Services;

public interface IDriverRepository : IGenericRepository<Driver>
{
}

public sealed class DriverRepository : GenericRepository<Driver>, IDriverRepository
{
    private readonly DriversManagementContext _dbContext;

    public DriverRepository(DriversManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

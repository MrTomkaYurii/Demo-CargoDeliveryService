namespace DriversManagement.Domain.Trucks.Services;

using DriversManagement.Domain.Trucks;
using DriversManagement.Databases;
using DriversManagement.Services;

public interface ITruckRepository : IGenericRepository<Truck>
{
}

public sealed class TruckRepository : GenericRepository<Truck>, ITruckRepository
{
    private readonly DriversManagementContext _dbContext;

    public TruckRepository(DriversManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

namespace DriversManagement.Domain.Repairs.Services;

using DriversManagement.Domain.Repairs;
using DriversManagement.Databases;
using DriversManagement.Services;

public interface IRepairRepository : IGenericRepository<Repair>
{
}

public sealed class RepairRepository : GenericRepository<Repair>, IRepairRepository
{
    private readonly DriversManagementContext _dbContext;

    public RepairRepository(DriversManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

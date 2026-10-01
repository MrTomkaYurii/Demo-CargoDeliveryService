namespace DriversManagement.Domain.Expences.Services;

using DriversManagement.Domain.Expences;
using DriversManagement.Databases;
using DriversManagement.Services;

public interface IExpenceRepository : IGenericRepository<Expence>
{
}

public sealed class ExpenceRepository : GenericRepository<Expence>, IExpenceRepository
{
    private readonly DriversManagementContext _dbContext;

    public ExpenceRepository(DriversManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

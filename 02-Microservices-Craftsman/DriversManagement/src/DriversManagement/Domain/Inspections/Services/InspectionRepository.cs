namespace DriversManagement.Domain.Inspections.Services;

using DriversManagement.Domain.Inspections;
using DriversManagement.Databases;
using DriversManagement.Services;

public interface IInspectionRepository : IGenericRepository<Inspection>
{
}

public sealed class InspectionRepository : GenericRepository<Inspection>, IInspectionRepository
{
    private readonly DriversManagementContext _dbContext;

    public InspectionRepository(DriversManagementContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

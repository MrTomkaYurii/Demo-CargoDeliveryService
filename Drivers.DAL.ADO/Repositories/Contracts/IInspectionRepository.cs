using Drivers.DAL.ADO.Entities;

namespace Drivers.DAL.ADO.Repositories.Contracts;

public interface IInspectionRepository : IGenericRepository<Inspection>
{
    Task<IEnumerable<Inspection>> GetInspectionsByTruckIdAsync(int truckId);
}

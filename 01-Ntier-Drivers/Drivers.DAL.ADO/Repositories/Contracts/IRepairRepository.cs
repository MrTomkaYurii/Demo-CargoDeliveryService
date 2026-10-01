using Drivers.DAL.ADO.Entities;

namespace Drivers.DAL.ADO.Repositories.Contracts
{
    public interface IRepairRepository : IGenericRepository<Repair>
    {
        Task<IEnumerable<Repair>> GetRepairsByTruckIdAsync(int truckId);
    }
}

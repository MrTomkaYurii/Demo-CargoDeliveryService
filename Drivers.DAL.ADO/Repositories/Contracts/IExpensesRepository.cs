using Drivers.DAL.ADO.Entities;

namespace Drivers.DAL.ADO.Repositories.Contracts;

public interface IExpensesRepository : IGenericRepository<Expenses>
{
    Task<IEnumerable<Expenses>> GetExpensesByDriverIdAsync(int driverId);
    Task<IEnumerable<Expenses>> GetExpensesByTruckIdAsync(int truckId);
}

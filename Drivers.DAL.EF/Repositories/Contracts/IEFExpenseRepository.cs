using Drivers.DAL.EF.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.DAL.EF.Repositories.Contracts
{
    public interface IEFExpenseRepository : IEFGenericRepository<EFExpense>
    {
        Task<IEnumerable<EFExpense>> GetExpencesByDriver(int driver_id);
        Task<IEnumerable<EFExpense>> GetExpensesByDriverIdAsync(int driverId);
        Task<IEnumerable<EFExpense>> GetExpensesByTruckIdAsync(int truckId);
    }
}

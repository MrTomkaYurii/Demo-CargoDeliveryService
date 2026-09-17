using Drivers.DAL.EF.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.DAL.EF.Repositories.Contracts
{
    public interface IEFRepairRepository : IEFGenericRepository<EFRepair>
    {
        Task<IEnumerable<EFRepair>> GetRepairsByTruckIdAsync(int truckId);
    }
}

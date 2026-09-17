using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Entities.HelpModels;

namespace Drivers.DAL.EF.Repositories.Contracts;

/// <summary>
/// Інтерфейс репозиторію вантажівок для Entity Framework Core
/// </summary>
public interface IEFTruckRepository : IEFGenericRepository<EFTruck>
{
    /// <summary>
    /// Зведена аналітика вартості обслуговування, ремонтів та результатів техоглядів
    /// </summary>
    Task<TruckMaintenanceSummary?> GetTruckMaintenanceSummaryAsync(int truckId);
}

using Drivers.DAL.ADO.Entities;
using Drivers.DAL.ADO.Entities.HelpModels;

namespace Drivers.DAL.ADO.Repositories.Contracts;

/// <summary>
/// Інтерфейс репозиторію вантажівок для ADO.NET / Dapper
/// </summary>
public interface ITruckRepository : IGenericRepository<Truck>
{
    /// <summary>
    /// Отримання повної інформації про вантажівку з історією витрат, ремонтів та техоглядів
    /// </summary>
    Task<Truck?> GetCompleteEntityAsync(int id);

    /// <summary>
    /// Зведена аналітика вартості обслуговування, ремонтів та результатів техоглядів
    /// </summary>
    Task<TruckMaintenanceSummary?> GetTruckMaintenanceSummaryAsync(int truckId);
}

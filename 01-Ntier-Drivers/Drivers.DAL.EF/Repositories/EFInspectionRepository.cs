using Drivers.DAL.EF.Data;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Drivers.DAL.EF.Repositories;

/// <summary>
/// Репозиторій техоглядів для Entity Framework Core
/// </summary>
public class EFInspectionRepository : EFGenericRepository<EFInspection>, IEFInspectionRepository
{
    public EFInspectionRepository(DriversManagementContext databaseContext) : base(databaseContext)
    {
    }

    /// <summary>
    /// Отримання повної інформації про техогляд разом із вантажівкою
    /// </summary>
    public override async Task<EFInspection?> GetCompleteEntityAsync(int id)
    {
        return await table
            .AsNoTracking()
            .Include(i => i.Truck)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    /// <summary>
    /// Отримання списку техоглядів для конкретної вантажівки
    /// </summary>
    public async Task<IEnumerable<EFInspection>> GetInspectionsByTruckIdAsync(int truckId)
    {
        return await table
            .AsNoTracking()
            .Where(i => i.TruckId == truckId)
            .ToListAsync();
    }
}

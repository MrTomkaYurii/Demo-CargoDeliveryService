using Drivers.DAL.EF.Data;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Drivers.DAL.EF.Repositories;

/// <summary>
/// Репозиторій ремонтів для Entity Framework Core
/// </summary>
public class EFRepairRepository : EFGenericRepository<EFRepair>, IEFRepairRepository
{
    public EFRepairRepository(DriversManagementContext databaseContext) : base(databaseContext)
    {
    }

    /// <summary>
    /// Отримання повної інформації про ремонт разом із вантажівкою
    /// </summary>
    public override async Task<EFRepair?> GetCompleteEntityAsync(int id, CancellationToken cancellationToken = default)
    {
        return await table
            .AsNoTracking()
            .Include(r => r.Truck)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <summary>
    /// Отримання списку ремонтів для конкретної вантажівки
    /// </summary>
    public async Task<IEnumerable<EFRepair>> GetRepairsByTruckIdAsync(int truckId)
    {
        return await table
            .AsNoTracking()
            .Where(r => r.TruckId == truckId)
            .OrderByDescending(r => r.RepairDate)
            .ToListAsync();
    }
}

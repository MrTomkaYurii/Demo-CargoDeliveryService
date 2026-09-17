using Drivers.DAL.EF.Data;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Drivers.DAL.EF.Repositories;

/// <summary>
/// Репозиторій витрат для Entity Framework Core
/// </summary>
public class EFExpenseRepository : EFGenericRepository<EFExpense>, IEFExpenseRepository
{
    public EFExpenseRepository(DriversManagementContext databaseContext) : base(databaseContext)
    {
    }

    /// <summary>
    /// Отримання повної інформації про запис витрат разом із водієм та закріпленою вантажівкою
    /// </summary>
    public override async Task<EFExpense?> GetCompleteEntityAsync(int id)
    {
        return await table
            .AsNoTracking()
            .Include(e => e.Driver)
            .Include(e => e.Truck)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <summary>
    /// Отримання всіх витрат за конкретним водієм
    /// </summary>
    public async Task<IEnumerable<EFExpense>> GetExpencesByDriver(int driver_id)
    {
        return await table
            .AsNoTracking()
            .Where(e => e.DriverId == driver_id)
            .ToListAsync();
    }

    /// <summary>
    /// Отримання всіх витрат за ідентифікатором водія (асинхронно)
    /// </summary>
    public async Task<IEnumerable<EFExpense>> GetExpensesByDriverIdAsync(int driverId)
    {
        return await GetExpencesByDriver(driverId);
    }

    /// <summary>
    /// Отримання всіх витрат за ідентифікатором вантажівки
    /// </summary>
    public async Task<IEnumerable<EFExpense>> GetExpensesByTruckIdAsync(int truckId)
    {
        return await table
            .AsNoTracking()
            .Where(e => e.TruckId == truckId)
            .ToListAsync();
    }
}

using Drivers.DAL.EF.Data;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Entities.HelpModels;
using Drivers.DAL.EF.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Drivers.DAL.EF.Repositories;

/// <summary>
/// Репозиторій вантажівок для Entity Framework Core.
/// Забезпечує Eager Loading пов'язаних сутностей, аналітику стану автопарку та наскрізне скасування через CancellationToken.
/// </summary>
public class EFTruckRepository : EFGenericRepository<EFTruck>, IEFTruckRepository
{
    public EFTruckRepository(DriversManagementContext databaseContext) : base(databaseContext)
    {
    }

    /// <summary>
    /// Отримання повної інформації про вантажівку разом із пов'язаними витратами, ремонтами та інспекціями
    /// </summary>
    public override async Task<EFTruck?> GetCompleteEntityAsync(int id, CancellationToken cancellationToken = default)
    {
        return await table
            .AsNoTracking()
            .Include(t => t.Expenses.OrderByDescending(e => e.Date))
            .Include(t => t.Repairs.OrderByDescending(r => r.RepairDate))
            .Include(t => t.Inspections.OrderByDescending(i => i.InspectionDate))
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <summary>
    /// Отримання аналітики витрат на обслуговування, ремонтів та інспекцій вантажівки
    /// </summary>
    public async Task<TruckMaintenanceSummary?> GetTruckMaintenanceSummaryAsync(int truckId)
    {
        return await table
            .AsNoTracking()
            .Where(t => t.Id == truckId)
            .Select(t => new TruckMaintenanceSummary
            {
                TruckId = t.Id,
                TruckNumber = t.TruckNumber,
                Model = t.Model,
                TotalMaintenanceCost = t.Expenses.Sum(e => (decimal?)e.MaintenanceCost) ?? 0m,
                TotalRepairsCost = t.Repairs.Sum(r => (decimal?)r.Cost) ?? 0m,
                TotalInspectionsCount = t.Inspections.Count(),
                PassedInspectionsCount = t.Inspections.Count(i => i.Result),
                FailedInspectionsCount = t.Inspections.Count(i => !i.Result)
            })
            .FirstOrDefaultAsync();
    }
}

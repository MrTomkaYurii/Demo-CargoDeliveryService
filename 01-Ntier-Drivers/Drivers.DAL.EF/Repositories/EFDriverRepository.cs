using Drivers.DAL.EF.Data;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Entities.HelpModels;
using Drivers.DAL.EF.Helpers;
using Drivers.DAL.EF.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Drivers.DAL.EF.Repositories;

/// <summary>
/// Репозиторій водіїв на базі Entity Framework Core.
/// Реалізує LINQ-запити з оптимізованим AsNoTracking, Eager Loading (Include),
/// агрегацією даних та повною наскрізною підтримкою CancellationToken для негайного скасування запитів на рівні БД.
/// </summary>
public class EFDriverRepository : EFGenericRepository<EFDriver>, IEFDriverRepository
{
    private readonly ISortHelper<EFDriver> _sortHelper;

    public EFDriverRepository(
        DriversManagementContext databaseContext,
        ISortHelper<EFDriver> sortHelper)
        : base(databaseContext)
    {
        _sortHelper = sortHelper;
    }

    /// <summary>
    /// Отримання сутності водія за Id без відстеження контекстом змін (No-Tracking)
    /// </summary>
    public async Task<EFDriver?> GetByIdAsNoTrackingAsync(int id, CancellationToken cancellationToken = default)
    {
        return await table.AsNoTracking().FirstOrDefaultAsync(drv => drv.Id == id, cancellationToken);
    }

    /// <summary>
    /// Отримання повної інформації про водія разом із компанією, фотографією та списком витрат (з деталями вантажівки).
    /// Використовує Eager Loading через Include/ThenInclude та підтримує скасування через CancellationToken.
    /// </summary>
    public override async Task<EFDriver?> GetCompleteEntityAsync(int id, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Drivers
            .AsNoTracking()
            .Include(driver => driver.Company)
            .Include(driver => driver.Photo)
            .Include(driver => driver.Expenses.OrderByDescending(e => e.Date))
                .ThenInclude(expense => expense.Truck)
            .FirstOrDefaultAsync(driver => driver.Id == id, cancellationToken);
    }

    /// <summary>
    /// Отримання списку водіїв із пагінацією, фільтрацією за роками народження та прізвищем,
    /// динамічним сортуванням та асинхронним скасуванням запиту через CancellationToken.
    /// </summary>
    public async Task<PagedList<EFDriver>> GetPaginatedDriversAsync(
        DriverParameters driverParameters, 
        CancellationToken cancellationToken = default)
    {
        var query = table.AsNoTracking().AsQueryable();

        if (driverParameters.MinYearOfBirth > 0)
        {
            query = query.Where(d => d.Birthdate.Year >= driverParameters.MinYearOfBirth);
        }

        if (driverParameters.MaxYearOfBirth > 0)
        {
            query = query.Where(d => d.Birthdate.Year <= driverParameters.MaxYearOfBirth);
        }

        if (!string.IsNullOrWhiteSpace(driverParameters.LastName))
        {
            var lastName = driverParameters.LastName.Trim().ToLower();
            query = query.Where(d => d.LastName.ToLower().Contains(lastName));
        }

        var sortedQuery = _sortHelper.ApplySort(query, driverParameters.OrderBy);

        return await PagedList<EFDriver>.ToPagedListAsync(
            sortedQuery, 
            driverParameters.PageNumber, 
            driverParameters.PageSize, 
            cancellationToken);
    }

    /// <summary>
    /// Агрегований звіт витрат водія за обраний проміжок часу з підтримкою CancellationToken.
    /// Рахує суми виплат, пального, обслуговування та середні значення.
    /// </summary>
    public async Task<DriverExpenseSummary> GetDriverExpenseSummaryAsync(
        int driverId, 
        DateTime? fromDate, 
        DateTime? toDate, 
        CancellationToken cancellationToken = default)
    {
        var query = databaseContext.Expenses
            .AsNoTracking()
            .Where(e => e.DriverId == driverId);

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.Date <= toDate.Value);
        }

        var expenses = await query.ToListAsync(cancellationToken);

        if (expenses.Count == 0)
        {
            return new DriverExpenseSummary { DriverId = driverId };
        }

        var totalDriverPayment = expenses.Sum(e => e.DriverPayment);
        var totalFuelCost = expenses.Sum(e => e.FuelCost);
        var totalMaintenanceCost = expenses.Sum(e => e.MaintenanceCost);
        var grandTotal = totalDriverPayment + totalFuelCost + totalMaintenanceCost;

        return new DriverExpenseSummary
        {
            DriverId = driverId,
            ExpenseCount = expenses.Count,
            TotalDriverPayment = totalDriverPayment,
            TotalFuelCost = totalFuelCost,
            TotalMaintenanceCost = totalMaintenanceCost,
            GrandTotal = grandTotal,
            AverageExpense = grandTotal / expenses.Count,
            FirstExpenseDate = expenses.Min(e => e.Date),
            LastExpenseDate = expenses.Max(e => e.Date)
        };
    }

    /// <summary>
    /// Отримання Топ-N водіїв з найбільшими загальними витратами для компанії через GroupBy в LINQ з CancellationToken.
    /// </summary>
    public async Task<IEnumerable<TopDriverExpense>> GetTopDriversByExpensesAsync(
        int topCount, 
        CancellationToken cancellationToken = default)
    {
        var query = databaseContext.Expenses
            .AsNoTracking()
            .GroupBy(e => new
            {
                e.DriverId,
                e.Driver.FirstName,
                e.Driver.LastName,
                CompanyName = e.Driver.Company != null ? e.Driver.Company.CompanyName : null
            })
            .Select(g => new TopDriverExpense
            {
                DriverId = g.Key.DriverId,
                FullName = (g.Key.FirstName + " " + g.Key.LastName).Trim(),
                CompanyName = g.Key.CompanyName,
                TotalExpenses = g.Sum(x => x.DriverPayment + x.FuelCost + x.MaintenanceCost),
                ExpensesCount = g.Count(),
                LastExpenseDate = g.Max(x => (DateTime?)x.Date)
            })
            .OrderByDescending(x => x.TotalExpenses)
            .Take(topCount);

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Отримання водіїв, термін дії посвідчення яких закінчується протягом заданої кількості днів з CancellationToken.
    /// </summary>
    public async Task<IEnumerable<DriverLicenseAlert>> GetDriversWithExpiringLicenseAsync(
        int daysThreshold, 
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var thresholdDate = today.AddDays(daysThreshold);

        var drivers = await table
            .AsNoTracking()
            .Where(d => d.DriverLicenseExpirationDate >= today && d.DriverLicenseExpirationDate <= thresholdDate)
            .OrderBy(d => d.DriverLicenseExpirationDate)
            .ToListAsync(cancellationToken);

        return drivers.Select(d => new DriverLicenseAlert
        {
            DriverId = d.Id,
            FullName = $"{d.FirstName} {d.LastName}".Trim(),
            DriverLicenseNumber = d.DriverLicenseNumber,
            DriverLicenseCategory = d.DriverLicenseCategory,
            DriverLicenseExpirationDate = d.DriverLicenseExpirationDate,
            DaysUntilExpiration = (int)(d.DriverLicenseExpirationDate.Date - today).TotalDays,
            Phone = d.Phone,
            Email = d.Email
        }).ToList();
    }
}

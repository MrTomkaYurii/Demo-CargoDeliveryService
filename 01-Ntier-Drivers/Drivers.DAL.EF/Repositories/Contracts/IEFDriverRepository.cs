using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Entities.HelpModels;
using Drivers.DAL.EF.Helpers;

namespace Drivers.DAL.EF.Repositories.Contracts;

/// <summary>
/// Інтерфейс репозиторію водіїв для Entity Framework Core.
/// Задає контракт вибірки та агрегації даних із повною підтримкою наскрізного CancellationToken.
/// </summary>
public interface IEFDriverRepository : IEFGenericRepository<EFDriver>
{
    /// <summary>
    /// Отримання сутності водія за Id без відстеження контекстом EF Core (AsNoTracking)
    /// </summary>
    Task<EFDriver?> GetByIdAsNoTrackingAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримання списку водіїв з фільтрацією за віком, прізвищем, сортуванням та пагінацією
    /// </summary>
    Task<PagedList<EFDriver>> GetPaginatedDriversAsync(DriverParameters driverParameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Агрегований звіт витрат водія за обраний проміжок часу
    /// </summary>
    Task<DriverExpenseSummary> GetDriverExpenseSummaryAsync(int driverId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Топ-N водіїв за загальною сумою витрат (виплати, паливо, ремонти)
    /// </summary>
    Task<IEnumerable<TopDriverExpense>> GetTopDriversByExpensesAsync(int topCount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Список водіїв, термін дії посвідчення яких закінчується протягом заданої кількості днів
    /// </summary>
    Task<IEnumerable<DriverLicenseAlert>> GetDriversWithExpiringLicenseAsync(int daysThreshold, CancellationToken cancellationToken = default);
}
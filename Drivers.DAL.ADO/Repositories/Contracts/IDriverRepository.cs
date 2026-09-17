using Drivers.DAL.ADO.Entities;
using Drivers.DAL.ADO.Entities.HelpModels;
using Drivers.DAL.ADO.Helpers;

namespace Drivers.DAL.ADO.Repositories.Contracts;

/// <summary>
/// Інтерфейс репозиторію водіїв для ADO.NET / Dapper.
/// Задає контракт для вибірки та агрегації даних, еквівалентний EF Core реалізації.
/// </summary>
public interface IDriverRepository : IGenericRepository<Driver>
{
    /// <summary>
    /// Отримання повної інформації про водія разом із компанією, фото та історією витрат
    /// </summary>
    Task<Driver?> GetCompleteEntityAsync(int id);

    /// <summary>
    /// Отримання водія за Id
    /// </summary>
    Task<Driver?> GetByIdAsync(int id);

    /// <summary>
    /// Отримання списку водіїв з фільтрацією за віком, прізвищем, сортуванням та пагінацією
    /// </summary>
    Task<PagedList<Driver>> GetPaginatedDriversAsync(DriverParameters driverParameters);

    /// <summary>
    /// Агрегований звіт витрат водія за обраний проміжок часу
    /// </summary>
    Task<DriverExpenseSummary> GetDriverExpenseSummaryAsync(int driverId, DateTime? fromDate, DateTime? toDate);

    /// <summary>
    /// Топ-N водіїв за загальною сумою витрат (виплати, паливо, ремонти)
    /// </summary>
    Task<IEnumerable<TopDriverExpense>> GetTopDriversByExpensesAsync(int topCount);

    /// <summary>
    /// Список водіїв, термін дії посвідчення яких закінчується протягом заданої кількості днів
    /// </summary>
    Task<IEnumerable<DriverLicenseAlert>> GetDriversWithExpiringLicenseAsync(int daysThreshold);
}

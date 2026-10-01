using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Models;

namespace WPFDrivers.Services;

/// <summary>
/// Інтерфейс клієнта для взаємодії з розподіленим REST Web API (DriversApi)
/// </summary>
public interface IDriversApiClient
{
    /// <summary>
    /// Отримання повної інформації про водія через HTTP REST
    /// </summary>
    Task<FullDriverResponceDTO> GetFullDriverInfoAsync(
        int id, 
        DataAccessProvider provider = DataAccessProvider.EntityFramework, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Порівняння продуктивності EF Core та Dapper через HTTP REST
    /// </summary>
    Task<QueryComparisonResultDTO<FullDriverResponceDTO>> CompareDriverProvidersAsync(
        int id, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримання агрегованої фінансової аналітики витрат водія через HTTP REST
    /// </summary>
    Task<DriverExpenseSummaryResponseDTO> GetDriverExpenseSummaryAsync(
        int id, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        DataAccessProvider provider = DataAccessProvider.EntityFramework, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримання водіїв з терміном дії ліцензії, що закінчується, через HTTP REST
    /// </summary>
    Task<IEnumerable<DriverLicenseAlertResponseDTO>> GetExpiringLicensesAsync(
        int daysThreshold = 30, 
        DataAccessProvider provider = DataAccessProvider.EntityFramework, 
        CancellationToken cancellationToken = default);
}

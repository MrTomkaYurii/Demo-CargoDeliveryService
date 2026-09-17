using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Models;

namespace Drivers.BLL.Managers.Contracts;

/// <summary>
/// Інтерфейс менеджера управління вантажівками
/// </summary>
public interface ITruckManager
{
    Task<IEnumerable<TruckResponceDTO>> GetAllTrucks(
        DataAccessProvider provider, 
        CancellationToken cancellationToken);

    Task<TruckMaintenanceSummaryResponseDTO?> GetTruckMaintenanceSummaryAsync(
        int truckId, 
        DataAccessProvider provider, 
        CancellationToken cancellationToken);

    Task<QueryComparisonResultDTO<TruckMaintenanceSummaryResponseDTO>> CompareTruckMaintenancePerformanceAsync(
        int truckId, 
        CancellationToken cancellationToken);
}

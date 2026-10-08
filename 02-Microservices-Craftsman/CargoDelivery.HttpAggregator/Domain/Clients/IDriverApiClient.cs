namespace CargoDelivery.HttpAggregator.Domain.Clients;

using CargoDelivery.HttpAggregator.Domain.Dtos;

/// <summary>
/// Інтерфейс типізованого HTTP-клієнта до мікросервісу DriversManagement.
/// </summary>
public interface IDriverApiClient
{
    Task<AggregatedDriverDto?> GetDriverByIdAsync(Guid driverId, CancellationToken cancellationToken = default);
}

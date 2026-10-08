namespace CargoDelivery.HttpAggregator.Domain.Services;

using CargoDelivery.HttpAggregator.Domain.Dtos;

/// <summary>
/// Інтерфейс диспетчерського сервісу агрегації доставки.
/// </summary>
public interface IShipmentAggregatorService
{
    Task<ShipmentSummaryDto> GetShipmentSummaryAsync(Guid driverId, CancellationToken cancellationToken = default);
}

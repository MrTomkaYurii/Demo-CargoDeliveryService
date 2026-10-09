namespace CargoDelivery.HttpAggregator.Domain.Dtos;

/// <summary>
/// Агреговані дані доставки, отримані з мікросервісу CargoOrderingService
/// (ендпоінт api/internal/deliveries?driverId=...).
/// Імена властивостей збігаються з JSON-полями відповіді (camelCase ↔ PascalCase
/// System.Text.Json зіставляє автоматично завдяки JsonSerializerDefaults.Web).
/// </summary>
public sealed class AggregatedCargoOrderDto
{
    public Guid OrderId { get; set; }
    public string CargoDescription { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public string DeliveryDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

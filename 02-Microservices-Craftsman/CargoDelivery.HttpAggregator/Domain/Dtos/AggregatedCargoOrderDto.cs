namespace CargoDelivery.HttpAggregator.Domain.Dtos;

/// <summary>
/// Агреговані дані замовлення / доставки, отримані з мікросервісу CargoOrderingService.
/// </summary>
public sealed class AggregatedCargoOrderDto
{
    public Guid OrderId { get; set; }
    public string CargoDescription { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public string OriginCity { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

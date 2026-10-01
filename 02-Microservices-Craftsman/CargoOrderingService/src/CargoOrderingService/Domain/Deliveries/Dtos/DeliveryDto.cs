namespace CargoOrderingService.Domain.Deliveries.Dtos;

using Destructurama.Attributed;

public sealed record DeliveryDto
{
    public Guid Id { get; set; }
    public string DeliveryDate { get; set; }
    public string PickupAddress { get; set; }
    public string DestinationAddress { get; set; }
    public string PackageDetails { get; set; }
    public string DeliveryStatus { get; set; }

}

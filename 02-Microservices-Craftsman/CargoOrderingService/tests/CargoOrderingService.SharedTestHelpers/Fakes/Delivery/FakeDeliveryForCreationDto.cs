namespace CargoOrderingService.SharedTestHelpers.Fakes.Delivery;

using AutoBogus;
using CargoOrderingService.Domain.Deliveries;
using CargoOrderingService.Domain.Deliveries.Dtos;

public sealed class FakeDeliveryForCreationDto : AutoFaker<DeliveryForCreationDto>
{
    public FakeDeliveryForCreationDto()
    {
    }
}
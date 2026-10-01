namespace CargoOrderingService.SharedTestHelpers.Fakes.Delivery;

using AutoBogus;
using CargoOrderingService.Domain.Deliveries;
using CargoOrderingService.Domain.Deliveries.Models;

public sealed class FakeDeliveryForUpdate : AutoFaker<DeliveryForUpdate>
{
    public FakeDeliveryForUpdate()
    {
    }
}
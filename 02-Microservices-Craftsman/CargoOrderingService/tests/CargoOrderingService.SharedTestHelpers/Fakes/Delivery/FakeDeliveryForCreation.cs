namespace CargoOrderingService.SharedTestHelpers.Fakes.Delivery;

using AutoBogus;
using CargoOrderingService.Domain.Deliveries;
using CargoOrderingService.Domain.Deliveries.Models;

public sealed class FakeDeliveryForCreation : AutoFaker<DeliveryForCreation>
{
    public FakeDeliveryForCreation()
    {
    }
}
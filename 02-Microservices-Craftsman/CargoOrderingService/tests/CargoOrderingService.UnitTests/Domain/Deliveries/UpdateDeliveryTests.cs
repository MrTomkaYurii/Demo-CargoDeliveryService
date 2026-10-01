namespace CargoOrderingService.UnitTests.Domain.Deliveries;

using CargoOrderingService.SharedTestHelpers.Fakes.Delivery;
using CargoOrderingService.Domain.Deliveries;
using CargoOrderingService.Domain.Deliveries.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = CargoOrderingService.Exceptions.ValidationException;

public class UpdateDeliveryTests
{
    private readonly Faker _faker;

    public UpdateDeliveryTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_delivery()
    {
        // Arrange
        var delivery = new FakeDeliveryBuilder().Build();
        var updatedDelivery = new FakeDeliveryForUpdate().Generate();
        
        // Act
        delivery.Update(updatedDelivery);

        // Assert
        delivery.DeliveryDate.Should().Be(updatedDelivery.DeliveryDate);
        delivery.PickupAddress.Should().Be(updatedDelivery.PickupAddress);
        delivery.DestinationAddress.Should().Be(updatedDelivery.DestinationAddress);
        delivery.PackageDetails.Should().Be(updatedDelivery.PackageDetails);
        delivery.DeliveryStatus.Should().Be(updatedDelivery.DeliveryStatus);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var delivery = new FakeDeliveryBuilder().Build();
        var updatedDelivery = new FakeDeliveryForUpdate().Generate();
        delivery.DomainEvents.Clear();
        
        // Act
        delivery.Update(updatedDelivery);

        // Assert
        delivery.DomainEvents.Count.Should().Be(1);
        delivery.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(DeliveryUpdated));
    }
}
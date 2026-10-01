namespace CargoOrderingService.IntegrationTests.FeatureTests.Deliveries;

using CargoOrderingService.SharedTestHelpers.Fakes.Delivery;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CargoOrderingService.Domain.Deliveries.Features;

public class AddDeliveryCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_delivery_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var deliveryOne = new FakeDeliveryForCreationDto().Generate();

        // Act
        var command = new AddDelivery.Command(deliveryOne);
        var deliveryReturned = await testingServiceScope.SendAsync(command);
        var deliveryCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Deliveries
            .FirstOrDefaultAsync(d => d.Id == deliveryReturned.Id));

        // Assert
        deliveryReturned.DeliveryDate.Should().Be(deliveryOne.DeliveryDate);
        deliveryReturned.PickupAddress.Should().Be(deliveryOne.PickupAddress);
        deliveryReturned.DestinationAddress.Should().Be(deliveryOne.DestinationAddress);
        deliveryReturned.PackageDetails.Should().Be(deliveryOne.PackageDetails);
        deliveryReturned.DeliveryStatus.Should().Be(deliveryOne.DeliveryStatus);

        deliveryCreated.DeliveryDate.Should().Be(deliveryOne.DeliveryDate);
        deliveryCreated.PickupAddress.Should().Be(deliveryOne.PickupAddress);
        deliveryCreated.DestinationAddress.Should().Be(deliveryOne.DestinationAddress);
        deliveryCreated.PackageDetails.Should().Be(deliveryOne.PackageDetails);
        deliveryCreated.DeliveryStatus.Should().Be(deliveryOne.DeliveryStatus);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddDelivery);
        var deliveryOne = new FakeDeliveryForCreationDto();

        // Act
        var command = new AddDelivery.Command(deliveryOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
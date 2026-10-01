namespace CargoOrderingService.IntegrationTests.FeatureTests.Deliveries;

using CargoOrderingService.SharedTestHelpers.Fakes.Delivery;
using CargoOrderingService.Domain.Deliveries.Dtos;
using CargoOrderingService.Domain.Deliveries.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateDeliveryCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_delivery_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var delivery = new FakeDeliveryBuilder().Build();
        await testingServiceScope.InsertAsync(delivery);
        var updatedDeliveryDto = new FakeDeliveryForUpdateDto().Generate();

        // Act
        var command = new UpdateDelivery.Command(delivery.Id, updatedDeliveryDto);
        await testingServiceScope.SendAsync(command);
        var updatedDelivery = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Deliveries
                .FirstOrDefaultAsync(d => d.Id == delivery.Id));

        // Assert
        updatedDelivery.DeliveryDate.Should().Be(updatedDeliveryDto.DeliveryDate);
        updatedDelivery.PickupAddress.Should().Be(updatedDeliveryDto.PickupAddress);
        updatedDelivery.DestinationAddress.Should().Be(updatedDeliveryDto.DestinationAddress);
        updatedDelivery.PackageDetails.Should().Be(updatedDeliveryDto.PackageDetails);
        updatedDelivery.DeliveryStatus.Should().Be(updatedDeliveryDto.DeliveryStatus);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdateDelivery);
        var deliveryOne = new FakeDeliveryForUpdateDto();

        // Act
        var command = new UpdateDelivery.Command(Guid.NewGuid(), deliveryOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
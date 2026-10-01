namespace CargoOrderingService.IntegrationTests.FeatureTests.Deliveries;

using CargoOrderingService.SharedTestHelpers.Fakes.Delivery;
using CargoOrderingService.Domain.Deliveries.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class DeliveryQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_delivery_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var deliveryOne = new FakeDeliveryBuilder().Build();
        await testingServiceScope.InsertAsync(deliveryOne);

        // Act
        var query = new GetDelivery.Query(deliveryOne.Id);
        var delivery = await testingServiceScope.SendAsync(query);

        // Assert
        delivery.DeliveryDate.Should().Be(deliveryOne.DeliveryDate);
        delivery.PickupAddress.Should().Be(deliveryOne.PickupAddress);
        delivery.DestinationAddress.Should().Be(deliveryOne.DestinationAddress);
        delivery.PackageDetails.Should().Be(deliveryOne.PackageDetails);
        delivery.DeliveryStatus.Should().Be(deliveryOne.DeliveryStatus);
    }

    [Fact]
    public async Task get_delivery_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetDelivery.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadDelivery);

        // Act
        var command = new GetDelivery.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
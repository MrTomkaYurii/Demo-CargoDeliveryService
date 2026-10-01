namespace CargoOrderingService.IntegrationTests.FeatureTests.Deliveries;

using CargoOrderingService.SharedTestHelpers.Fakes.Delivery;
using CargoOrderingService.Domain.Deliveries.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeleteDeliveryCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_delivery_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var delivery = new FakeDeliveryBuilder().Build();
        await testingServiceScope.InsertAsync(delivery);

        // Act
        var command = new DeleteDelivery.Command(delivery.Id);
        await testingServiceScope.SendAsync(command);
        var deliveryResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Deliveries
                .CountAsync(d => d.Id == delivery.Id));

        // Assert
        deliveryResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_delivery_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteDelivery.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_delivery_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var delivery = new FakeDeliveryBuilder().Build();
        await testingServiceScope.InsertAsync(delivery);

        // Act
        var command = new DeleteDelivery.Command(delivery.Id);
        await testingServiceScope.SendAsync(command);
        var deletedDelivery = await testingServiceScope.ExecuteDbContextAsync(db => db.Deliveries
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == delivery.Id));

        // Assert
        deletedDelivery?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeleteDelivery);

        // Act
        var command = new DeleteDelivery.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
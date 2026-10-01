namespace CargoOrderingService.FunctionalTests.FunctionalTests.Deliveries;

using CargoOrderingService.SharedTestHelpers.Fakes.Delivery;
using CargoOrderingService.FunctionalTests.TestUtilities;
using CargoOrderingService.Domain;
using System.Net;
using System.Threading.Tasks;

public class UpdateDeliveryRecordTests : TestBase
{
    [Fact]
    public async Task put_delivery_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var delivery = new FakeDeliveryBuilder().Build();
        var updatedDeliveryDto = new FakeDeliveryForUpdateDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(delivery);

        // Act
        var route = ApiRoutes.Deliveries.Put(delivery.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedDeliveryDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task put_delivery_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var delivery = new FakeDeliveryBuilder().Build();
        var updatedDeliveryDto = new FakeDeliveryForUpdateDto { }.Generate();

        // Act
        var route = ApiRoutes.Deliveries.Put(delivery.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedDeliveryDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task put_delivery_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var delivery = new FakeDeliveryBuilder().Build();
        var updatedDeliveryDto = new FakeDeliveryForUpdateDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Deliveries.Put(delivery.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedDeliveryDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
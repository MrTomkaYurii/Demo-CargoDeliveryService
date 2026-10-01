namespace CargoOrderingService.FunctionalTests.FunctionalTests.Deliveries;

using CargoOrderingService.SharedTestHelpers.Fakes.Delivery;
using CargoOrderingService.FunctionalTests.TestUtilities;
using CargoOrderingService.Domain;
using System.Net;
using System.Threading.Tasks;

public class DeleteDeliveryTests : TestBase
{
    [Fact]
    public async Task delete_delivery_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var delivery = new FakeDeliveryBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(delivery);

        // Act
        var route = ApiRoutes.Deliveries.Delete(delivery.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task delete_delivery_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var delivery = new FakeDeliveryBuilder().Build();

        // Act
        var route = ApiRoutes.Deliveries.Delete(delivery.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task delete_delivery_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var delivery = new FakeDeliveryBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Deliveries.Delete(delivery.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
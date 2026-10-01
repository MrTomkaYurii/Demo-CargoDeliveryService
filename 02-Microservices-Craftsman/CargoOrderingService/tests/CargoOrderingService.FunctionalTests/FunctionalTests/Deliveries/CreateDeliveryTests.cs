namespace CargoOrderingService.FunctionalTests.FunctionalTests.Deliveries;

using CargoOrderingService.SharedTestHelpers.Fakes.Delivery;
using CargoOrderingService.FunctionalTests.TestUtilities;
using CargoOrderingService.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateDeliveryTests : TestBase
{
    [Fact]
    public async Task create_delivery_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var delivery = new FakeDeliveryForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.Deliveries.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, delivery);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_delivery_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var delivery = new FakeDeliveryForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.Deliveries.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, delivery);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_delivery_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var delivery = new FakeDeliveryForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Deliveries.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, delivery);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
namespace CargoOrderingService.FunctionalTests.FunctionalTests.Orders;

using CargoOrderingService.SharedTestHelpers.Fakes.Order;
using CargoOrderingService.FunctionalTests.TestUtilities;
using CargoOrderingService.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateOrderTests : TestBase
{
    [Fact]
    public async Task create_order_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var order = new FakeOrderForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.Orders.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, order);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_order_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var order = new FakeOrderForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.Orders.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, order);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_order_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var order = new FakeOrderForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Orders.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, order);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
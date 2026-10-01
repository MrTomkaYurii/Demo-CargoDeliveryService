namespace CargoOrderingService.FunctionalTests.FunctionalTests.Orders;

using CargoOrderingService.SharedTestHelpers.Fakes.Order;
using CargoOrderingService.FunctionalTests.TestUtilities;
using CargoOrderingService.Domain;
using System.Net;
using System.Threading.Tasks;

public class DeleteOrderTests : TestBase
{
    [Fact]
    public async Task delete_order_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var order = new FakeOrderBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(order);

        // Act
        var route = ApiRoutes.Orders.Delete(order.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task delete_order_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var order = new FakeOrderBuilder().Build();

        // Act
        var route = ApiRoutes.Orders.Delete(order.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task delete_order_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var order = new FakeOrderBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Orders.Delete(order.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
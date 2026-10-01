namespace CargoOrderingService.FunctionalTests.FunctionalTests.Orders;

using CargoOrderingService.SharedTestHelpers.Fakes.Order;
using CargoOrderingService.FunctionalTests.TestUtilities;
using CargoOrderingService.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetOrderTests : TestBase
{
    [Fact]
    public async Task get_order_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var order = new FakeOrderBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(order);

        // Act
        var route = ApiRoutes.Orders.GetRecord(order.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_order_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var order = new FakeOrderBuilder().Build();

        // Act
        var route = ApiRoutes.Orders.GetRecord(order.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_order_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var order = new FakeOrderBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Orders.GetRecord(order.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
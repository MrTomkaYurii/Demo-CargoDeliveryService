namespace DriversManagement.FunctionalTests.FunctionalTests.Repairs;

using DriversManagement.SharedTestHelpers.Fakes.Repair;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetRepairTests : TestBase
{
    [Fact]
    public async Task get_repair_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var repair = new FakeRepairBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(repair);

        // Act
        var route = ApiRoutes.Repairs.GetRecord(repair.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_repair_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var repair = new FakeRepairBuilder().Build();

        // Act
        var route = ApiRoutes.Repairs.GetRecord(repair.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_repair_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var repair = new FakeRepairBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Repairs.GetRecord(repair.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
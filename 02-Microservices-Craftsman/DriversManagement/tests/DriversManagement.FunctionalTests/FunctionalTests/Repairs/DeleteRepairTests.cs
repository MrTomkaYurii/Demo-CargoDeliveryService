namespace DriversManagement.FunctionalTests.FunctionalTests.Repairs;

using DriversManagement.SharedTestHelpers.Fakes.Repair;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class DeleteRepairTests : TestBase
{
    [Fact]
    public async Task delete_repair_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var repair = new FakeRepairBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(repair);

        // Act
        var route = ApiRoutes.Repairs.Delete(repair.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task delete_repair_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var repair = new FakeRepairBuilder().Build();

        // Act
        var route = ApiRoutes.Repairs.Delete(repair.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task delete_repair_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var repair = new FakeRepairBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Repairs.Delete(repair.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
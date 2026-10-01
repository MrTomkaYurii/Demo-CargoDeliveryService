namespace DriversManagement.FunctionalTests.FunctionalTests.Repairs;

using DriversManagement.SharedTestHelpers.Fakes.Repair;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateRepairTests : TestBase
{
    [Fact]
    public async Task create_repair_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var repair = new FakeRepairForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.Repairs.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, repair);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_repair_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var repair = new FakeRepairForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.Repairs.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, repair);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_repair_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var repair = new FakeRepairForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Repairs.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, repair);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
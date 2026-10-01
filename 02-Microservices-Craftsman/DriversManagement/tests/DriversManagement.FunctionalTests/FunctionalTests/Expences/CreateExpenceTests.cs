namespace DriversManagement.FunctionalTests.FunctionalTests.Expences;

using DriversManagement.SharedTestHelpers.Fakes.Expence;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateExpenceTests : TestBase
{
    [Fact]
    public async Task create_expence_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var expence = new FakeExpenceForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.Expences.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, expence);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_expence_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var expence = new FakeExpenceForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.Expences.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, expence);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_expence_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var expence = new FakeExpenceForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Expences.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, expence);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
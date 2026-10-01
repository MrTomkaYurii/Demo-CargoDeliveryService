namespace CargoOrderingService.FunctionalTests.FunctionalTests.Users;

using CargoOrderingService.SharedTestHelpers.Fakes.User;
using CargoOrderingService.FunctionalTests.TestUtilities;
using CargoOrderingService.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateUserTests : TestBase
{
    [Fact]
    public async Task create_user_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var user = new FakeUserForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.Users.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, user);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_user_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var user = new FakeUserForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.Users.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, user);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_user_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var user = new FakeUserForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Users.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, user);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
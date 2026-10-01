namespace DriversBlogManagement.FunctionalTests.FunctionalTests.Users;

using DriversBlogManagement.SharedTestHelpers.Fakes.User;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetUserTests : TestBase
{
    [Fact]
    public async Task get_user_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var user = new FakeUserBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(user);

        // Act
        var route = ApiRoutes.Users.GetRecord(user.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_user_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var user = new FakeUserBuilder().Build();

        // Act
        var route = ApiRoutes.Users.GetRecord(user.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_user_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var user = new FakeUserBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Users.GetRecord(user.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
namespace DriversBlogManagement.FunctionalTests.FunctionalTests.PostAboutDrivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class DeletePostAboutDriverTests : TestBase
{
    [Fact]
    public async Task delete_postaboutdriver_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(postAboutDriver);

        // Act
        var route = ApiRoutes.PostAboutDrivers.Delete(postAboutDriver.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task delete_postaboutdriver_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();

        // Act
        var route = ApiRoutes.PostAboutDrivers.Delete(postAboutDriver.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task delete_postaboutdriver_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.PostAboutDrivers.Delete(postAboutDriver.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
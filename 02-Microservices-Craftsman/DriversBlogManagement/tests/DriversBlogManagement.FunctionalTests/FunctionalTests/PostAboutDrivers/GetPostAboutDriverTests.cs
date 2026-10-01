namespace DriversBlogManagement.FunctionalTests.FunctionalTests.PostAboutDrivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetPostAboutDriverTests : TestBase
{
    [Fact]
    public async Task get_postaboutdriver_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(postAboutDriver);

        // Act
        var route = ApiRoutes.PostAboutDrivers.GetRecord(postAboutDriver.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_postaboutdriver_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();

        // Act
        var route = ApiRoutes.PostAboutDrivers.GetRecord(postAboutDriver.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_postaboutdriver_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.PostAboutDrivers.GetRecord(postAboutDriver.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
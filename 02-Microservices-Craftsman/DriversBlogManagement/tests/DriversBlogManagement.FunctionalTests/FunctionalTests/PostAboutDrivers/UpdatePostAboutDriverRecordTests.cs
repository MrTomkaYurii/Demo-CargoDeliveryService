namespace DriversBlogManagement.FunctionalTests.FunctionalTests.PostAboutDrivers;

using DriversBlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class UpdatePostAboutDriverRecordTests : TestBase
{
    [Fact]
    public async Task put_postaboutdriver_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();
        var updatedPostAboutDriverDto = new FakePostAboutDriverForUpdateDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(postAboutDriver);

        // Act
        var route = ApiRoutes.PostAboutDrivers.Put(postAboutDriver.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedPostAboutDriverDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task put_postaboutdriver_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();
        var updatedPostAboutDriverDto = new FakePostAboutDriverForUpdateDto { }.Generate();

        // Act
        var route = ApiRoutes.PostAboutDrivers.Put(postAboutDriver.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedPostAboutDriverDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task put_postaboutdriver_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var postAboutDriver = new FakePostAboutDriverBuilder().Build();
        var updatedPostAboutDriverDto = new FakePostAboutDriverForUpdateDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.PostAboutDrivers.Put(postAboutDriver.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedPostAboutDriverDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
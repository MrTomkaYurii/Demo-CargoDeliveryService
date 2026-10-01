namespace DriversBlogManagement.FunctionalTests.FunctionalTests.Likes;

using DriversBlogManagement.SharedTestHelpers.Fakes.Like;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateLikeTests : TestBase
{
    [Fact]
    public async Task create_like_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var like = new FakeLikeForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.Likes.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, like);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_like_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var like = new FakeLikeForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.Likes.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, like);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_like_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var like = new FakeLikeForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Likes.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, like);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
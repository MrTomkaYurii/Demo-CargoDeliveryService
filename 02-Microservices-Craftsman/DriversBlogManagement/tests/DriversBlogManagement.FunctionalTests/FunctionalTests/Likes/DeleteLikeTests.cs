namespace DriversBlogManagement.FunctionalTests.FunctionalTests.Likes;

using DriversBlogManagement.SharedTestHelpers.Fakes.Like;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class DeleteLikeTests : TestBase
{
    [Fact]
    public async Task delete_like_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var like = new FakeLikeBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(like);

        // Act
        var route = ApiRoutes.Likes.Delete(like.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task delete_like_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var like = new FakeLikeBuilder().Build();

        // Act
        var route = ApiRoutes.Likes.Delete(like.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task delete_like_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var like = new FakeLikeBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Likes.Delete(like.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
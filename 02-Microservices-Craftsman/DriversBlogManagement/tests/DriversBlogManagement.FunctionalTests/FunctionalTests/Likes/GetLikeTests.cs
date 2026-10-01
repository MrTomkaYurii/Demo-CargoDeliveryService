namespace DriversBlogManagement.FunctionalTests.FunctionalTests.Likes;

using DriversBlogManagement.SharedTestHelpers.Fakes.Like;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetLikeTests : TestBase
{
    [Fact]
    public async Task get_like_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var like = new FakeLikeBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(like);

        // Act
        var route = ApiRoutes.Likes.GetRecord(like.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_like_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var like = new FakeLikeBuilder().Build();

        // Act
        var route = ApiRoutes.Likes.GetRecord(like.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_like_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var like = new FakeLikeBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Likes.GetRecord(like.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
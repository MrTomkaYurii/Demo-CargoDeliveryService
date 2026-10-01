namespace DriversBlogManagement.FunctionalTests.FunctionalTests.Comments;

using DriversBlogManagement.SharedTestHelpers.Fakes.Comment;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateCommentTests : TestBase
{
    [Fact]
    public async Task create_comment_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var comment = new FakeCommentForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.Comments.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, comment);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_comment_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var comment = new FakeCommentForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.Comments.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, comment);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_comment_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var comment = new FakeCommentForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Comments.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, comment);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
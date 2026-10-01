namespace DriversBlogManagement.FunctionalTests.FunctionalTests.Comments;

using DriversBlogManagement.SharedTestHelpers.Fakes.Comment;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class DeleteCommentTests : TestBase
{
    [Fact]
    public async Task delete_comment_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var comment = new FakeCommentBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(comment);

        // Act
        var route = ApiRoutes.Comments.Delete(comment.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task delete_comment_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var comment = new FakeCommentBuilder().Build();

        // Act
        var route = ApiRoutes.Comments.Delete(comment.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task delete_comment_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var comment = new FakeCommentBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Comments.Delete(comment.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
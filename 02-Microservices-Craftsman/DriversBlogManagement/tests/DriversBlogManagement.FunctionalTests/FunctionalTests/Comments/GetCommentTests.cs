namespace DriversBlogManagement.FunctionalTests.FunctionalTests.Comments;

using DriversBlogManagement.SharedTestHelpers.Fakes.Comment;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetCommentTests : TestBase
{
    [Fact]
    public async Task get_comment_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var comment = new FakeCommentBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(comment);

        // Act
        var route = ApiRoutes.Comments.GetRecord(comment.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_comment_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var comment = new FakeCommentBuilder().Build();

        // Act
        var route = ApiRoutes.Comments.GetRecord(comment.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_comment_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var comment = new FakeCommentBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Comments.GetRecord(comment.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
namespace DriversBlogManagement.FunctionalTests.FunctionalTests.Comments;

using DriversBlogManagement.SharedTestHelpers.Fakes.Comment;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class UpdateCommentRecordTests : TestBase
{
    [Fact]
    public async Task put_comment_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var comment = new FakeCommentBuilder().Build();
        var updatedCommentDto = new FakeCommentForUpdateDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(comment);

        // Act
        var route = ApiRoutes.Comments.Put(comment.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedCommentDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task put_comment_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var comment = new FakeCommentBuilder().Build();
        var updatedCommentDto = new FakeCommentForUpdateDto { }.Generate();

        // Act
        var route = ApiRoutes.Comments.Put(comment.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedCommentDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task put_comment_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var comment = new FakeCommentBuilder().Build();
        var updatedCommentDto = new FakeCommentForUpdateDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Comments.Put(comment.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedCommentDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
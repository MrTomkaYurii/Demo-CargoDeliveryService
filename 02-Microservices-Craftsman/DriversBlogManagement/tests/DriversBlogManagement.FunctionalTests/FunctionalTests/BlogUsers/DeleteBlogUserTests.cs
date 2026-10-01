namespace DriversBlogManagement.FunctionalTests.FunctionalTests.BlogUsers;

using DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class DeleteBlogUserTests : TestBase
{
    [Fact]
    public async Task delete_bloguser_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var blogUser = new FakeBlogUserBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(blogUser);

        // Act
        var route = ApiRoutes.BlogUsers.Delete(blogUser.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task delete_bloguser_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var blogUser = new FakeBlogUserBuilder().Build();

        // Act
        var route = ApiRoutes.BlogUsers.Delete(blogUser.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task delete_bloguser_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var blogUser = new FakeBlogUserBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.BlogUsers.Delete(blogUser.Id);
        var result = await FactoryClient.DeleteRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
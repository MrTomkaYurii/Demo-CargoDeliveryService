namespace DriversBlogManagement.FunctionalTests.FunctionalTests.BlogUsers;

using DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateBlogUserTests : TestBase
{
    [Fact]
    public async Task create_bloguser_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var blogUser = new FakeBlogUserForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.BlogUsers.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, blogUser);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_bloguser_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var blogUser = new FakeBlogUserForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.BlogUsers.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, blogUser);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_bloguser_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var blogUser = new FakeBlogUserForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.BlogUsers.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, blogUser);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
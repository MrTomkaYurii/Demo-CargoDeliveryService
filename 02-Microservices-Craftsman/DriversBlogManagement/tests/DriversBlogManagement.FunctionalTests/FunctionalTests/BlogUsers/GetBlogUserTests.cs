namespace DriversBlogManagement.FunctionalTests.FunctionalTests.BlogUsers;

using DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;
using DriversBlogManagement.FunctionalTests.TestUtilities;
using DriversBlogManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetBlogUserTests : TestBase
{
    [Fact]
    public async Task get_bloguser_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var blogUser = new FakeBlogUserBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(blogUser);

        // Act
        var route = ApiRoutes.BlogUsers.GetRecord(blogUser.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_bloguser_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var blogUser = new FakeBlogUserBuilder().Build();

        // Act
        var route = ApiRoutes.BlogUsers.GetRecord(blogUser.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_bloguser_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var blogUser = new FakeBlogUserBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.BlogUsers.GetRecord(blogUser.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
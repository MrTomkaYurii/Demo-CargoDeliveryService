namespace DriversBlogManagement.IntegrationTests.FeatureTests.BlogUsers;

using DriversBlogManagement.Domain.BlogUsers.Dtos;
using DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;
using DriversBlogManagement.Domain.BlogUsers.Features;
using Domain;
using System.Threading.Tasks;

public class BlogUserListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_bloguser_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var blogUserOne = new FakeBlogUserBuilder().Build();
        var blogUserTwo = new FakeBlogUserBuilder().Build();
        var queryParameters = new BlogUserParametersDto();

        await testingServiceScope.InsertAsync(blogUserOne, blogUserTwo);

        // Act
        var query = new GetBlogUserList.Query(queryParameters);
        var blogUsers = await testingServiceScope.SendAsync(query);

        // Assert
        blogUsers.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadUsers);
        var queryParameters = new BlogUserParametersDto();

        // Act
        var command = new GetBlogUserList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
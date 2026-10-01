namespace DriversBlogManagement.IntegrationTests.FeatureTests.BlogUsers;

using DriversBlogManagement.Domain.BlogUsers.Dtos;
using DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;
using DriversBlogManagement.Domain.BlogUsers.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllBlogUsersQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_blogusers()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var blogUserOne = new FakeBlogUserBuilder().Build();
        var blogUserTwo = new FakeBlogUserBuilder().Build();

        await testingServiceScope.InsertAsync(blogUserOne, blogUserTwo);

        // Act
        var query = new GetAllBlogUsers.Query();
        var blogUsers = await testingServiceScope.SendAsync(query);

        // Assert
        blogUsers.Count.Should().BeGreaterThanOrEqualTo(2);
        blogUsers.FirstOrDefault(x => x.Id == blogUserOne.Id).Should().NotBeNull();
        blogUsers.FirstOrDefault(x => x.Id == blogUserTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadUsers);

        // Act
        var query = new GetAllBlogUsers.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
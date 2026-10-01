namespace DriversBlogManagement.IntegrationTests.FeatureTests.BlogUsers;

using DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;
using DriversBlogManagement.Domain.BlogUsers.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class BlogUserQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_bloguser_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var blogUserOne = new FakeBlogUserBuilder().Build();
        await testingServiceScope.InsertAsync(blogUserOne);

        // Act
        var query = new GetBlogUser.Query(blogUserOne.Id);
        var blogUser = await testingServiceScope.SendAsync(query);

        // Assert
        blogUser.UserName.Should().Be(blogUserOne.UserName);
        blogUser.Email.Should().Be(blogUserOne.Email);
    }

    [Fact]
    public async Task get_bloguser_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetBlogUser.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadUser);

        // Act
        var command = new GetBlogUser.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}
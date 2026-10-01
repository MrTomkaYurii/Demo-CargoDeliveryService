namespace DriversBlogManagement.IntegrationTests.FeatureTests.BlogUsers;

using DriversBlogManagement.SharedTestHelpers.Fakes.BlogUser;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversBlogManagement.Domain.BlogUsers.Features;

public class AddBlogUserCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_bloguser_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var blogUserOne = new FakeBlogUserForCreationDto().Generate();

        // Act
        var command = new AddBlogUser.Command(blogUserOne);
        var blogUserReturned = await testingServiceScope.SendAsync(command);
        var blogUserCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.BlogUsers
            .FirstOrDefaultAsync(b => b.Id == blogUserReturned.Id));

        // Assert
        blogUserReturned.UserName.Should().Be(blogUserOne.UserName);
        blogUserReturned.Email.Should().Be(blogUserOne.Email);

        blogUserCreated.UserName.Should().Be(blogUserOne.UserName);
        blogUserCreated.Email.Should().Be(blogUserOne.Email);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddUser);
        var blogUserOne = new FakeBlogUserForCreationDto();

        // Act
        var command = new AddBlogUser.Command(blogUserOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}